using DataModel.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Reactive.Linq;
using System.Diagnostics;
using System.Reactive.Concurrency;

namespace DataModel.Client
{
    public class ClientMapBufferHandler
    {
        readonly IEventBus eventBus;
        public ClientMapBufferHandler(IEventBus bus) => eventBus = bus;

        public IDisposable AttachToBus()
        {

            //Tiles
            //var onlyValid = eventBus.GetEventStream<DataSourceEvent>()
            //                        .ParseOnlyValidUsingErrorHandler<ServerMapEvent>(ClientFunctions.PrintConsoleErrorHandler);
            var onlyValid = eventBus.GetEventStream<ServerMapEvent>();
            var serverContent = eventBus.GetEventStream<DataSourceEvent>()
                                        .ParseOnlyValidUsingErrorHandler<ServerContentEvent>(ClientFunctions.PrintConsoleErrorHandler)
                                        .Do(v => Console.WriteLine("RECEIVED" +  v.Content.Key + "    " + v.Content.Value));

            var bigTiles = from e in onlyValid
                           where e.Tiles != null
                           from tile in e.Tiles
                           select tile.MiniTiles;

            var smallTiles = from e in onlyValid
                             where e.MiniTiles != null
                             select e.MiniTiles;


            //Concats large updates with small updates
            var concat = from e in bigTiles.Merge(smallTiles)
                         .Where(v => v != null)
                         .Buffer(TimeSpan.FromSeconds(1))
                         select e.SelectMany(v => v).GroupBy(v => v.MiniTileId).ToDictionary(v => v.Key, v => v.First());


            //Client
            var latestClient = ClientFunctions.LatestClientLocation(eventBus.GetEventStream<UserGpsEvent>());



            //Setup buffers:
            var largeMapBuffer = from e in latestClient.DistinctUntilChanged()
                                        select new HashSet<PlusCode>(e.Neighbors(50));

            var smallUnityBuffer = from location in latestClient.DistinctUntilChanged()
                                          select (location.Neighbors(10) , location);


            var tileContent = eventBus.GetEventStream<DataSourceEvent>()
                                      .ParseOnlyValidUsingErrorHandler<ServerTileContentEvent>(ClientFunctions.PrintConsoleErrorHandler)
                                      .Where(v => v.VisibleContent != null)
                                      .StartWith(new ServerTileContentEvent());
                                     

            var clientPositionPlusContent = largeMapBuffer.CombineLatest(tileContent, (position, content) => new { position, content });


            var bufferedContent = clientPositionPlusContent.Scan(new Dictionary<PlusCode, List<ITileContent>>(), (buffer, val) =>
            {
                //Remove values far away:
                var farAway = from keyValue in buffer.Keys
                              where !val.position.Contains(keyValue)
                              select keyValue;

                farAway.ToList().ForEach(v => buffer.Remove(v));

                //Merge content overwriting old values on conflict:
                if (val.content.VisibleContent != null)
                {
                    val.content.VisibleContent.GroupBy(v => v.Key).ToDictionary(v => v.Key, v => v.First().Value).ToList().ForEach(x => buffer[x.Key] = x.Value);
                }
                return buffer;
            });




            return UpdateClientBufferWithBakedInTileContent(concat, latestClient, bufferedContent)
                .SubscribeOn(TaskPoolScheduler.Default)
                .CombineLatest(smallUnityBuffer, (buffer, renderDistance) => new { buffer, renderDistance })
                .Select(v => UnityRenderMap(v.renderDistance.Item1, v.buffer, v.renderDistance.location))
                .Where(v=>v.NullTiles == 0)
                .DistinctUntilChanged()
                .ObserveOn(DefaultScheduler.Instance)
                .Subscribe(v => eventBus.Publish(v));
        }




        IObservable<Dictionary<PlusCode, MiniTile>> UpdateClientBufferWithBakedInTileContent(IObservable<Dictionary<PlusCode, MiniTile>> bufferedMiniTileStream, IObservable<PlusCode> location, IObservable<Dictionary<PlusCode, List<ITileContent>>> tileContent)
        {
            return location.DistinctUntilChanged()
                                 .CombineLatest(bufferedMiniTileStream.DistinctUntilChanged(), (loc, tiles) => new { loc, tiles })
                                 .Scan(new Dictionary<PlusCode, MiniTile>(), (dict, val) =>
                                  {
                                      dict = TileGenerator.RegenerateArea(val.loc, dict, val.tiles, 40);
                                      return dict;
                                  })
                                 .DistinctUntilChanged()
                                 .CombineLatest(tileContent, (tiles, content) => new { tiles, content })
                                 .Select(v => SetMinitileContent(v.tiles, v.content));
        }

        

        static Dictionary<PlusCode, MiniTile> SetMinitileContent(Dictionary<PlusCode, MiniTile> map, Dictionary<PlusCode, List<ITileContent>> content)
        {
            if (content == null)
                return map;

            content.ToList().ForEach(v =>
            {
                MiniTile tile = null;
                map.TryGetValue(v.Key, out tile);
                if (tile != null)
                {
                    tile.Content = v.Value;
                }
            });
            return map;
        }

        public static MapAsRenderAbleChanged UnityRenderMap(List<PlusCode> visibleMap, Dictionary<PlusCode, MiniTile> buffer, PlusCode location)
        {
            var sortedList = new List<MiniTile>();
            int nullTiles = 0;

            visibleMap.ForEach(c =>
            {
                var tile = c.GetMiniTile(buffer);
                if (tile != null)
                {
                    sortedList.Add(tile);
                }
                else
                {
                    nullTiles++;
                    sortedList.Add(new MiniTile(c, MiniTileType.Unknown_Tile, null));
                }
            });
            return new MapAsRenderAbleChanged() { Location = location, Map = sortedList, NullTiles = nullTiles };

        }

    }
}

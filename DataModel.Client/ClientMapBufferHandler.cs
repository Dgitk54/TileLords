using DataModel.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Reactive.Linq;
using System.Diagnostics;

namespace DataModel.Client
{
    public class ClientMapBufferHandler
    {
        readonly IEventBus eventBus;
        public ClientMapBufferHandler(IEventBus bus) => eventBus = bus;

        public IDisposable AttachToBus()
        {

            //Tiles
            var onlyValid = eventBus.GetEventStream<DataSourceEvent>()
                                    .ParseOnlyValidUsingErrorHandler<ServerMapEvent>(ClientFunctions.PrintConsoleErrorHandler);
                                   



            var BigTiles = from e in onlyValid
                           where e.Tiles != null
                           from tile in e.Tiles
                           select tile.MiniTiles;

            var smallTiles = from e in onlyValid
                             where e.MiniTiles != null
                             select e.MiniTiles;


            var concat = from e in BigTiles.Merge(smallTiles).Where(v => v != null).Buffer(TimeSpan.FromSeconds(4))
                         select e.SelectMany(v => v).GroupBy(v => v.MiniTileId).ToDictionary(v => v.Key, v => v.First());



            //Client
            var latestClient = ClientFunctions.LatestClientLocation(eventBus.GetEventStream<UserGpsEvent>());

            var clientBuffer = from e in latestClient.DistinctUntilChanged()
                               select new HashSet<PlusCode>(e.Neighbors(50));


            //Content
            var tileContent = eventBus.GetEventStream<DataSourceEvent>()
                                      .ParseOnlyValidUsingErrorHandler<ServerTileContentEvent>(ClientFunctions.PrintConsoleErrorHandler)
                                      .Where(v => v.VisibleContent != null)
                                      .StartWith(new ServerTileContentEvent())
                                      .Do(v=> { if (v.VisibleContent != null) { Console.WriteLine(v.VisibleContent.Count); } });




            


                



            var clientPositionPlusContent = clientBuffer.CombineLatest(tileContent, (position, content) => new { position, content });



            //https://stackoverflow.com/questions/294138/merging-dictionaries-in-c-sharp
            var bufferedContent = clientPositionPlusContent.Scan(new Dictionary<PlusCode, List<ITileContent>>(), (buffer, val) =>
            {

                //Remove values far away:
                var farAway = from keyValue in buffer.Keys
                              where !val.position.Contains(keyValue)
                              select keyValue;
                
                farAway.ToList().ForEach(v => buffer.Remove(v));

                if(val.content.VisibleContent != null)
                {
                    val.content.VisibleContent.GroupBy(v => v.Key).ToDictionary(v => v.Key, v => v.First().Value).ToList().ForEach(x => buffer[x.Key] = x.Value);
                }
                return buffer;
            });


            return Accumulated(concat, latestClient, bufferedContent).Subscribe(v => eventBus.Publish(new ClientMapBufferChanged(v)));
        }






        IObservable<Dictionary<PlusCode, MiniTile>> Accumulated(IObservable<Dictionary<PlusCode, MiniTile>> bufferedMiniTileStream, IObservable<PlusCode> location, IObservable<Dictionary<PlusCode,List<ITileContent>>> tileContent)
        {
            var output = location.DistinctUntilChanged().CombineLatest(bufferedMiniTileStream.DistinctUntilChanged(), (loc, tiles) => new { loc, tiles })
                                 .Scan(new Dictionary<PlusCode, MiniTile>(), (dict, val) =>
                                  {
                                      dict = TileGenerator.RegenerateArea(val.loc, dict, val.tiles, 40);
                                      return dict;
                                  })
                                 .DistinctUntilChanged()
                                 .CombineLatest(tileContent, (tiles, content) => new { tiles, content }).Select(v =>
                                 {
                                     SetMinitileContent(v.tiles, v.content);
                                     return v.tiles;
                                 });
            return output;

        }

        void SetMinitileContent(Dictionary<PlusCode, MiniTile> map, Dictionary<PlusCode, List<ITileContent>> content)
        {
            if (content == null)
                return;

            content.ToList().ForEach(v => 
            {
                MiniTile tile = null;
                map.TryGetValue(v.Key, out tile);
                if (tile != null) 
                {
                    tile.Content = v.Value;
                }
            });
        }


    }
}

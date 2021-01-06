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




            //Content
            var tileContent = eventBus.GetEventStream<DataSourceEvent>()
                                      .ParseOnlyValidUsingErrorHandler<ServerTileContentEvent>(ClientFunctions.PrintConsoleErrorHandler)
                                      .Where(v => v.VisibleContent != null)
                                      .StartWith(new ServerTileContentEvent());




            concat.CombineLatest(tileContent, (tiles, content) => new { tiles, content }).Select(v =>
            {
                SetMinitileContent(v.tiles, v.content.VisibleContent);
                return v.tiles;
            });


                



            var clientPositionPlusContent = latestClient.WithLatestFrom(tileContent, (position, content) => new { position, content });




            var bufferedContent = clientPositionPlusContent.Scan(new List<KeyValuePair<PlusCode, List<ITileContent>>>(), (buffer, val) =>
            {

                //Remove values far away:
                var currentClientLocation = val.position;




                var farAway = from keyValue in buffer
                              let key = keyValue.Key
                              where PlusCodeUtils.GetChebyshevDistance(key, currentClientLocation) > 50
                              select key;


                //farAway.ToList().ForEach(v => buffer.Remove(v));
                //return buffer.Concat(val.content.VisibleContent).GroupBy(v => v.Key).ToDictionary(v => v.Key, v => v.First().Value);
                if (val.content.VisibleContent == null)
                    return buffer;


                buffer.AddRange(val.content.VisibleContent);


                return buffer;

            });


            return Accumulated(concat, latestClient).Subscribe(v => eventBus.Publish(new ClientMapBufferChanged(v)));
        }






        IObservable<Dictionary<PlusCode, MiniTile>> Accumulated(IObservable<Dictionary<PlusCode, MiniTile>> bufferedMiniTileStream, IObservable<PlusCode> location)
        {
            var output = location.DistinctUntilChanged().CombineLatest(bufferedMiniTileStream.DistinctUntilChanged(), (loc, tiles) => new { loc, tiles })
                                 .Scan(new Dictionary<PlusCode, MiniTile>(), (dict, val) =>
                                  {
                                      dict = TileGenerator.RegenerateArea(val.loc, dict, val.tiles, 40);
                                      return dict;
                                  });

            return output.DistinctUntilChanged();

        }

        void SetMinitileContent(Dictionary<PlusCode, MiniTile> map, List<KeyValuePair<PlusCode, List<ITileContent>>> content)
        {
            content.ForEach(v =>
            {
                var tile = TileUtility.GetMiniTile(v.Key, map.Values.ToList());
                if (tile != null)
                {
                    string tileContent = "";
                    v.Value.ForEach(x =>
                    {
                        tileContent += x.ToString() + "  ";
                    });

                    Console.WriteLine("Adding tilecontent for " + v.Key + "  " + tileContent);
                    tile.Content = v.Value;
                }
            });
        }


    }
}

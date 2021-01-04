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
                         select e.SelectMany(v => v).ToList();



            //Client
            var latestClient = ClientFunctions.LatestClientLocation(eventBus.GetEventStream<UserGpsEvent>());




            //Content
            var tileContent = eventBus.GetEventStream<DataSourceEvent>()
                                      .ParseOnlyValidUsingErrorHandler<ServerTileContentEvent>(ClientFunctions.PrintConsoleErrorHandler)
                                      .Where(v => v.VisibleContent != null)
                                      .StartWith(new ServerTileContentEvent());



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


            return Accumulated(concat, latestClient, tileContent).Subscribe(v => eventBus.Publish(new ClientMapBufferChanged(v)));
        }






        IObservable<IList<MiniTile>> Accumulated(IObservable<IList<MiniTile>> bufferedMiniTileStream, IObservable<PlusCode> location, IObservable<ServerTileContentEvent> content)
        {
            var output = location.CombineLatest(bufferedMiniTileStream, (loc, tiles) => new { loc, tiles })
                                 .CombineLatest(content, (locTiles, serverTileContent) => new { locTiles, serverTileContent })
                                 .Scan(new List<MiniTile>(), (list, val) => 
                                 {
                                     list = TileGenerator.RegenerateArea(val.locTiles.loc, list, val.locTiles.tiles, 40);

                                     if(val.serverTileContent != null && val.serverTileContent.VisibleContent != null)
                                     {
                                         var copy = new List<KeyValuePair<PlusCode, List<ITileContent>>>(val.serverTileContent.VisibleContent);
                                         SetMinitileContent(list, copy);

                                     }
                                       
                                     return list;
                                 });

            return output.DistinctUntilChanged();

        }

        void SetMinitileContent(List<MiniTile> map, List<KeyValuePair<PlusCode, List<ITileContent>>> content)
        {
            content.ForEach(v =>
            {
                var tile = TileUtility.GetMiniTile(v.Key, map);
                if(tile != null)
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

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

            


            var allTiles = from e in onlyValid
                           where e.Tiles != null
                           from tile in e.Tiles
                           where tile != null
                           from bigUpdates in tile.MiniTiles
                           select bigUpdates;

            var smallUpdates = from e in onlyValid
                               where e.MiniTiles != null
                               from mini in e.MiniTiles
                               where mini != null
                               select mini;



            var merged = allTiles.Merge(smallUpdates).Where(v => v != null);


            var receivedLarge = BufferMiniTiles(merged);

            //Client
            var latestClient = ClientFunctions.LatestClientLocation(eventBus.GetEventStream<UserGpsEvent>());



            //Content

            var tileContent = eventBus.GetEventStream<DataSourceEvent>().ParseOnlyValidUsingErrorHandler<ServerTileContentEvent>(ClientFunctions.PrintConsoleErrorHandler);




            var clientPositionPlusContent = latestClient.WithLatestFrom(tileContent, (position, content) => new { position, content });



            var bufferedContent = clientPositionPlusContent.Scan(new Dictionary<PlusCode, ITileContent>(), (buffer, val) =>
            {
                //Remove values far away:
                var currentClientLocation = val.position;
                var farAway = from key in buffer.Keys
                              where PlusCodeUtils.GetChebyshevDistance(key, currentClientLocation) > 50
                              select key;


                farAway.ToList().ForEach(v => buffer.Remove(v));

                if (val.content.VisibleContent == null)
                    return buffer;
                return buffer.Concat(val.content.VisibleContent).GroupBy(v => v.Key).ToDictionary(v => v.Key, v => v.First().Value);
            });


            return MapBufferChanged(receivedLarge, latestClient, bufferedContent)
                .Subscribe(v => eventBus.Publish(new ClientMapBufferChanged(v)));
        }





        IObservable<IList<MiniTile>> MapBufferChanged(IObservable<IList<MiniTile>> observable1, IObservable<PlusCode> location, IObservable<Dictionary<PlusCode, ITileContent>> content)
        => Accumulated(observable1, location, content);

        IObservable<IList<MiniTile>> BufferMiniTiles(IObservable<MiniTile> observable) => observable.Buffer(TimeSpan.FromSeconds(2));


        IObservable<IList<MiniTile>> Accumulated(IObservable<IList<MiniTile>> bufferedMiniTileStream, IObservable<PlusCode> location, IObservable<Dictionary<PlusCode, ITileContent>> content)
        {
            var output = bufferedMiniTileStream
                .CombineLatest(location, (tiles, code) => new { tiles, code})
                .CombineLatest(content, (locbuf, tcontent) => new { locbuf, tcontent })
                .Scan(new List<MiniTile>(), (list, l1) =>
            {
                list = TileGenerator.RegenerateArea(l1.locbuf.code, list, l1.locbuf.tiles, 40);

                l1.tcontent.Keys.ToList().AsParallel().ForAll(v =>
                {
                    var tile = TileUtility.GetMiniTile(v, list);
                    ITileContent getOutDict;
                    l1.tcontent.TryGetValue(v, out getOutDict);
                    var contentToAdd = new List<ITileContent>() { getOutDict };
                    tile.Content = contentToAdd;
                });
                return list;
            });
            return output.DistinctUntilChanged();


        }



    }
}

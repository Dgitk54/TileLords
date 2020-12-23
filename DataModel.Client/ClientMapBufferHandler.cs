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





            return MapBufferChanged(concat, latestClient, bufferedContent)
                .Subscribe(v => eventBus.Publish(new ClientMapBufferChanged(v)));
        }





        IObservable<IList<MiniTile>> MapBufferChanged(IObservable<IList<MiniTile>> observable1, IObservable<PlusCode> location, IObservable<List<KeyValuePair<PlusCode, List<ITileContent>>>> content)
        => Accumulated(observable1, location, content);



        IObservable<IList<MiniTile>> Accumulated(IObservable<IList<MiniTile>> bufferedMiniTileStream, IObservable<PlusCode> location, IObservable<List<KeyValuePair<PlusCode, List<ITileContent>>>> content)
        {
            var output = bufferedMiniTileStream
                .CombineLatest(location, (tiles, code) => new { tiles, code })
                .CombineLatest(content, (locbuf, tcontent) => new { locbuf, tcontent })
                .Scan(new List<MiniTile>(), (list, l1) =>
            {
                list = TileGenerator.RegenerateArea(l1.locbuf.code, list, l1.locbuf.tiles, 40);
                var copy = new List<KeyValuePair<PlusCode, List<ITileContent>>>(l1.tcontent);
                SetMinitileContent(list, copy);
                return list;
            });
            return output.DistinctUntilChanged();


        }

        void SetMinitileContent(List<MiniTile> map, List<KeyValuePair<PlusCode, List<ITileContent>>> content)
        {
            content.ForEach(v =>
            {
                var tile = TileUtility.GetMiniTile(v.Key, map);
                tile.Content = v.Value;
            });
        }


    }
}

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
    public class MapForUnityHandler
    {
        readonly IEventBus eventBus;
        public MapForUnityHandler(IEventBus bus)
        {
            eventBus = bus;
        }
        public IDisposable AttachToBus()
        {
            //.Subscribe(v => Debug.WriteLine("CLIENTPOS" + v.Code))
            var latestClientLocation = ClientFunctions.LatestClientLocation(eventBus.GetEventStream<ClientGpsChangedEvent>());
            var buffer = eventBus.GetEventStream<ClientMapBufferChanged>();
            var combinedBufferAndLocation = buffer.WithLatestFrom(latestClientLocation, (buff, latestLocation) => new { buff, latestLocation });


            // var visibleMap = GetVisibleMap(ExtractData(eventBus.GetEventStream<ClientMapBufferChanged>()), latestClientLocation); //return .Subscribe(v=>Debug.WriteLine("GETVISIBLEMAP" + v.Count));
            var visibleMapWithLocation = from bufLoc in combinedBufferAndLocation
                                         select new { Lists = GetVisibleMap(bufLoc.buff.TilesToRenderForUnity, bufLoc.latestLocation), bufLoc.latestLocation };
            var sortedMap = from touple in visibleMapWithLocation
                            select new { SortedList = SortVisibleMap(touple.Lists), touple.latestLocation };
   
            
            var mapEvent = from pos in sortedMap
                           select new MapForUnityChanged(pos.latestLocation, pos.SortedList);
            return mapEvent.DistinctUntilChanged().Subscribe(v =>
            {
                // Console.WriteLine("MAPEVENT" + v.ClientLocation.Code + "      " + v.MiniTiles.Count);
                eventBus.Publish<MapForUnityChanged>(v);
            }
            );
        }

        /*  IDisposable Attach()
          {
              var latestClient = ClientFunctions.LatestClientLocation(eventBus.GetEventStream<ClientGpsChangedEvent>());
              return ToEvent(GetVisibleMap(ExtractData(eventBus.GetEventStream<ClientMapBufferChanged>()), latestClient), latestClient)
                  .Subscribe(v => eventBus.Publish<MapForUnityChanged>(v));
          } */

                                                                                                  


        IList<MiniTile> GetVisibleMap(IList<MiniTile> tiles, PlusCode currentLocation)
        {

            return TileUtility.GetMiniTileSectionWithinChebyshevDistance(currentLocation, tiles, 10);
        }


        IObservable<MapForUnityChanged> ToEvent(IObservable<IList<MiniTile>> miniTiles, IObservable<PlusCode> location)
            => from e in location
               from v in miniTiles
               select new MapForUnityChanged(e, v);

        IList<MiniTile> SortVisibleMap(IList<MiniTile> tiles)
        {

            return LocationCodeTileUtility.SortList(tiles);
        }
    }
}

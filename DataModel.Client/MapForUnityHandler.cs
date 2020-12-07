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
            var latestClientLocation = ClientFunctions.LatestClientLocation(eventBus.GetEventStream<UserGpsChangedEvent>());
            var buffer = eventBus.GetEventStream<ClientMapBufferChanged>();
            var combinedBufferAndLocation = buffer.WithLatestFrom(latestClientLocation, (buff, latestLocation) => new { buff, latestLocation });

            var visibleMapWithLocation = from bufLoc in combinedBufferAndLocation
                                         select new { Lists = GetVisibleMap(bufLoc.buff.TilesToRenderForUnity, bufLoc.latestLocation), bufLoc.latestLocation };
            var sortedMap = from touple in visibleMapWithLocation
                            select new { SortedList = SortVisibleMap(touple.Lists), touple.latestLocation };
   
            
            var mapEvent = from pos in sortedMap
                           select new MapForUnityChanged(pos.latestLocation, pos.SortedList);
            return mapEvent.DistinctUntilChanged().Subscribe(v =>
            {
                eventBus.Publish<MapForUnityChanged>(v);
            }
            );
        }

        

        IList<MiniTile> GetVisibleMap(IList<MiniTile> tiles, PlusCode currentLocation)
        {

            return TileUtility.GetMiniTileSectionWithinChebyshevDistance(currentLocation, tiles, 10);
        }



        IList<MiniTile> SortVisibleMap(IList<MiniTile> tiles)
        {

            return LocationCodeTileUtility.SortList(tiles);
        }
    }
}

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
            var latestClient = ClientFunctions.LatestClientLocation(eventBus.GetEventStream<ClientGpsChangedEvent>());
            var visibleMap = GetVisibleMap(ExtractData(eventBus.GetEventStream<ClientMapBufferChanged>()), latestClient); //return .Subscribe(v=>Debug.WriteLine("GETVISIBLEMAP" + v.Count));

            var mapEvent = from pos in latestClient
                           from map in visibleMap
                           select new MapForUnityChanged(pos, map);
            return mapEvent.DistinctUntilChanged().Subscribe(v => Console.WriteLine("MAPEVENT" + v.ClientLocation.Code + "      " + v.MiniTiles.Count));
        }

        IDisposable Attach()
        {
            var latestClient = ClientFunctions.LatestClientLocation(eventBus.GetEventStream<ClientGpsChangedEvent>());
            return ToEvent(GetVisibleMap(ExtractData(eventBus.GetEventStream<ClientMapBufferChanged>()), latestClient), latestClient)
                .Subscribe(v => eventBus.Publish<MapForUnityChanged>(v));
        }

        IObservable<IList<MiniTile>> ExtractData(IObservable<ClientMapBufferChanged> observable) => from e in observable
                                                                                                    select e.TilesToRenderForUnity;


        IObservable<IList<MiniTile>> GetVisibleMap(IObservable<IList<MiniTile>> tiles, IObservable<PlusCode> currentLocation)
        => from list in tiles
           from v in currentLocation
           select TileUtility.GetMiniTileSectionWithinChebyshevDistance(v, list, 20);



        IObservable<MapForUnityChanged> ToEvent(IObservable<IList<MiniTile>> miniTiles, IObservable<PlusCode> location)
            => from e in location
               from v in miniTiles
               select new MapForUnityChanged(e, v);
    }
}

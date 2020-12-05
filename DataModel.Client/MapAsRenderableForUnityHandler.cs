using DataModel.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Reactive.Linq;

namespace DataModel.Client
{
    public class MapAsRenderableForUnityHandler
    {

        readonly IEventBus eventBus;
        public MapAsRenderableForUnityHandler(IEventBus bus)
        {
            eventBus = bus;
        }
        public IDisposable AttachToBus()
        {
            var mapChangedStream = TransformToRenderable(eventBus.GetEventStream<MapForUnityChanged>());
            return mapChangedStream.Subscribe(v => eventBus.Publish(v));
        }

        IObservable<MapAsRenderAbleChanged> TransformToRenderable(IObservable<MapForUnityChanged> observable) => from e in observable
                                                                                                                 select TransformToRenderable(e);


        MapAsRenderAbleChanged TransformToRenderable(MapForUnityChanged map)
        {
            var neighbors = map.ClientLocation.Neighbors(10);
            var dict = new Dictionary<PlusCode, MiniTile>();

            neighbors.ForEach(v => dict.Add(v, v.GetMiniTile(map.MiniTiles)));
            return new MapAsRenderAbleChanged() { Location = map.ClientLocation, Map = dict };

        }
    }
}

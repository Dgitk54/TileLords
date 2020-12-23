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
            var latestClientLocation = ClientFunctions.LatestClientLocation(eventBus.GetEventStream<UserGpsEvent>());
            var latestBuffer = eventBus.GetEventStream<ClientMapBufferChanged>();


            return latestClientLocation.WithLatestFrom(latestBuffer, (loc, buffer) => new { loc, buffer }).DistinctUntilChanged()
                                       .Subscribe(v => 
                                       {
                                           var neighbors = v.loc.Neighbors(10);
                                           var dict = new Dictionary<PlusCode, MiniTile>();

                                           neighbors.ForEach(c => dict.Add(c, c.GetMiniTile(v.buffer.TilesToRenderForUnity)));

                                           var map = new MapAsRenderAbleChanged() { Location = v.loc, Map = dict };
                                           eventBus.Publish(map);
                                       });
        }

        
    }
}

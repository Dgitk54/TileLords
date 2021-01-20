using DataModel.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Reactive.Linq;
using System.Diagnostics;
using System.Reactive.Concurrency;

namespace DataModel.Client
{
    public class ClientLocalMapBufferHandler
    {

        IEventBus clientBus;


        public ClientLocalMapBufferHandler(IEventBus bus)
        {
            clientBus = bus;

        }


        public IDisposable AttachToBus()
        {
            return ClientFunctions.LatestClientAreaChange(clientBus.GetEventStream<UserGpsEvent>())
                                  .DistinctUntilChanged()
                                  .Select(v => LocationCodeTileUtility.GetTileSection(v.Code, 1, v.Precision))
                                  .Select(v => v.ConvertAll(e => new PlusCode(e, 8)))
                                  .Select(v => v.ConvertAll(e => WorldGenerator.GenerateTile(e)))
                                  .Select(v => new ServerMapEvent() { Tiles = v})
                                  .DistinctUntilChanged()
                                  .Subscribe(v => clientBus.Publish(v));
        }
    }
}

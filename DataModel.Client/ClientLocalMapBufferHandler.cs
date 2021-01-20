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
            var latestClient = ClientFunctions.LatestClientLocation(clientBus.GetEventStream<UserGpsEvent>());

            return null;
        }
    }
}

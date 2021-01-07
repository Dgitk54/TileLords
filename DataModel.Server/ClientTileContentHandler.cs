using DataModel.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.Server
{
    public class ClientTileContentHandler
    {



        readonly JsonSerializerSettings settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        };
        readonly IEventBus eventBus;
        public ClientTileContentHandler(IEventBus clientBus)
        {
            eventBus = clientBus;
        }

        public IDisposable AttachToBus()
        {

            return eventBus.GetEventStream<ServerTileContentEvent>().Subscribe(v =>
            {
                var serialized = JsonConvert.SerializeObject(v, typeof(ServerTileContentEvent), settings);
                var sink = new DataSinkEvent(serialized);
                eventBus.Publish(sink);
            });


        }

    }
}

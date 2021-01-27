using DataModel.Common;
using Google.OpenLocationCode;
using LiteDB;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading;

namespace DataModel.Server
{


    public class PlayerTileContentSender
    {
        readonly IEventBus clientBus;
        readonly JsonSerializerSettings settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        };
        public PlayerTileContentSender(IEventBus clientBus)
        {
            this.clientBus = clientBus;
        }

        public IDisposable AttachToBus()
        {
            return clientBus.GetEventStream<PlayerVisibleEvent>()
                     .Select(v => JsonConvert.SerializeObject(v, typeof(ServerContentEvent), settings))
                     .Select(v => new DataSinkEvent(v))
                     .Subscribe(v => clientBus.Publish(v));
        }
    }
}

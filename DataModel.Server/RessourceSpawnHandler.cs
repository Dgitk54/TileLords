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
    public class RessourceSpawnHandler
    {
        readonly IMessageBus eventBus;
        const int spawnPeriodInSeconds = 5;
        public RessourceSpawnHandler(IMessageBus serverBus)
        {
            
        }

        public IDisposable AttachToBus()
        {
            return null;
        }
    }
}

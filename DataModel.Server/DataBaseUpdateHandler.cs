using DataModel.Common;
using System;
using System.Collections.Generic;
using System.Text;
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
    /// <summary>
    /// Class responsible for intercepting database relevant server events and updating the database.
    /// </summary>
    public class DatabaseUpdateHandler
    {
        readonly IEventBus eventBus;
        public DatabaseUpdateHandler(IEventBus serverBus)
        {
            eventBus = serverBus;
        }
        public IDisposable AttachToBus()
        {
            var mapEvents = eventBus.GetEventStream<ServerMapEvent>();

            mapEvents.Subscribe(v =>
            {
                v.MiniTiles.ToList().ForEach(v2 => DataBaseFunctions.UpdateTile(v2));
            });

            return null;
        }
    }
}

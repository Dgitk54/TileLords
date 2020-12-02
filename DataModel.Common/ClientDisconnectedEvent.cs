using DataModel.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.Common
{
    public class ClientDisconnectedEvent : IEvent
    {

        public string DebugData { get; }
        public ClientDisconnectedEvent(string data) => DebugData = data;
        
    }
}

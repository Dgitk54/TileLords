using DataModel.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.Server
{
    public class ClientConnectedEvent : IEvent
    {
        public string DebugData { get; }


        public ClientConnectedEvent(string data)
        => DebugData = data;
    }
}

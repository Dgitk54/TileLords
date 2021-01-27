using DataModel.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.Common
{
    public class ClientDisconnectedEvent : IMessage
    {
        
        public string DebugData { get; }
        public ClientDisconnectedEvent(string data) => DebugData = data;


        public override string ToString()
        {
            return base.ToString() + "  " + DebugData;
        }

    }
}

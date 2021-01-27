using DataModel.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.Common
{
    public class ClientConnectedEvent : IMessage
    {
        public string DebugData { get; }


        public ClientConnectedEvent(string data)
        => DebugData = data;
        public override string ToString()
        {
            return base.ToString() + "  "+ DebugData;
        }
    }
}

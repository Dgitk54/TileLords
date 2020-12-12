using DataModel.Common;
using DotNetty.Transport.Channels;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.Server
{
    public class ServerClientDisconnectedEvent : IEvent
    {
        public IChannel Channel { get; set; }
    }
}

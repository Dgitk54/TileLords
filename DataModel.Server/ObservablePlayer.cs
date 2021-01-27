using DataModel.Common;
using DotNetty.Transport.Channels;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.Server
{
    public class ObservablePlayer
    {
        public string Name { get; set; }

        public int Id { get; set; }

        public IMessageBus ClientBus { get; set; }

        public IChannel ClientChannel { get; set; }

        public IObservable<PlusCode> PlayerObservableLocationStream { get; set; }

        public IObservable<bool> ConnectionStatus { get; set; }


    }
}

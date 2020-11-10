using DataModel.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.Server
{
    interface IClientConnectedMessage : INetworkMessage
    {
        IObservable<Client> ClientConnected { get; }

    }
}

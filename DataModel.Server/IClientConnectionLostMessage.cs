using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.Server
{
    public interface IClientConnectionLost 
    {
        IObservable<Client> ClientDisconnected { get; }
    }
}

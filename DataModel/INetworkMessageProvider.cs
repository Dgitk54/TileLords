
using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.Common
{
    /// <summary>
    /// Interface which is responsible to provide a stream of messages received from the network.
    /// </summary>
    interface INetworkMessageProvider
    {
        IObservable<INetworkMessage> NetworkMessageReceived { get; }
    }
}

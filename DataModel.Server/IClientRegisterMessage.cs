using DataModel.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.Server
{
    public interface IClientRegisterMessage : INetworkMessage
    {
         string PlayerCredentials { get; }
         string RegisterStatus { get; }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.Common
{
    public interface INetworkByteMessage
    {
        byte[] BytePayload { get; }
    }
}

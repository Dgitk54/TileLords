using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.Common
{
    /// <summary>
    /// Base interface implemented by all network messages.
    /// </summary>
    public interface INetworkMessage
    {
        string Message { get; }
    }
}

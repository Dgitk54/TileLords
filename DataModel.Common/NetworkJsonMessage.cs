using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.Common
{
    /// <summary>
    /// Base interface implemented by all network messages.
    /// </summary>
    public class NetworkJsonMessage
    {
        public string JsonPayload { get; }

        public NetworkJsonMessage(string message) => (JsonPayload) = message;
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.Common.Messages
{
    public class TransactionMessage : IMessage
    {
        public MessageType MessageType { get; set; }
        public MessageContext MessageContext { get; set; }
        public MessageState MessageState { get; set; }
        public byte[] TransactionFrom { get; set; }
        public byte[] TransactionTo { get; set; }
        public List<KeyValuePair<ResourceType, int>> TransactionAmount { get; set; }
    }
}

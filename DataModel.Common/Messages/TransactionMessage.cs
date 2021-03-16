using DataModel.Common.GameModel;
using MessagePack;
using System.Collections.Generic;

namespace DataModel.Common.Messages
{

    [MessagePackObject]
    public class TransactionMessage : IMessage
    {
        [Key(0)]
        public MessageType MessageType { get; set; }
        [Key(1)]
        public MessageContext MessageContext { get; set; }
        [Key(2)]
        public MessageState MessageState { get; set; }
        [Key(3)]
        public byte[] TransactionFrom { get; set; }
        [Key(4)]
        public byte[] TransactionTo { get; set; }
        [Key(5)]
        public List<KeyValuePair<ResourceType, int>> TransactionAmount { get; set; }
    }
}

using DataModel.Common.GameModel;
using MessagePack;
using System.Collections.Generic;

namespace DataModel.Common.Messages
{

    [MessagePackObject]
    public class InventoryContentMessage : IMessage
    {
        [Key(0)]
        public MessageType Type { get; set; }
        [Key(1)]
        public MessageState MessageState { get; set; }
        [Key(2)]
        public byte[] InventoryOwner { get; set; }
        [Key(3)]
        public List<KeyValuePair<InventoryType, int>> InventoryContent { get; set; }
    }
}

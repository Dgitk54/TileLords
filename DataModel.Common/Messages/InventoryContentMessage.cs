using DataModel.Common.GameModel;
using System.Collections.Generic;

namespace DataModel.Common.Messages
{
    public class InventoryContentMessage : IMessage
    {
        public MessageType Type { get; set; }
        public MessageState MessageState { get; set; }
        public byte[] InventoryOwner { get; set; }
        public List<KeyValuePair<InventoryType, int>> InventoryContent { get; set; }
    }
}

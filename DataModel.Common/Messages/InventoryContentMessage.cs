using DataModel.Common.GameModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.Common.Messages
{
    public class InventoryContentMessage : IMessage
    {
        public MessageType Type { get; set; }
        public MessageState MessageState { get; set; }
        public byte[] InventoryOwner { get; set; }
        public List<KeyValuePair<ItemType, int>> InventoryContent { get; set; }
    }
}

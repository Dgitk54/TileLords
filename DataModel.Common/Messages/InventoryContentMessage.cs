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
        public List<KeyValuePair<ResourceType, int>> InventoryContent { get; set; }
    }
}

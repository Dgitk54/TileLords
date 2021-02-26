using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.Common.Messages
{
    public class InventoryContentMessage
    {
        public byte[] InventoryOwner { get; set; }
        public List<KeyValuePair<ResourceType, int>> InventoryContent { get; set; }
    }
}

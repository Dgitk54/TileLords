using DataModel.Common.GameModel;
using DataModel.Common.Messages;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.Server.Model
{
    public class Inventory
    {
        public ObjectId InventoryId { get; set; }
        public byte[] OwnerId { get; set; }
        public byte[] ContainerId { get; set; }
        public int StorageCapacity { get; set; }
        public Dictionary<ItemType, int> ResourceDictionary { get; set; }
        
    }
}

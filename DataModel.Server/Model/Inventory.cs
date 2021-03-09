using DataModel.Common.GameModel;
using DataModel.Common.Messages;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.Server.Model
{
    public class Inventory : IUserInventory
    {
        public ObjectId InventoryId { get; set; }
        public byte[] OwnerId { get; set; }
        public byte[] ContainerId { get; set; }
        public int StorageCapacity { get; set; }
        public List<DatabaseInventoryStorage> InventoryItems { get; set; }
        Dictionary<InventoryType, int> IUserInventory.InventoryItems { get =>  InventoryItems.ToInventoryDictionary(); set { InventoryItems = value.ToDatabaseStorage(); } }
    }
}

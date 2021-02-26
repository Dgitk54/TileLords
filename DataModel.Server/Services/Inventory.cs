using DataModel.Common.Messages;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.Server.Services
{
    public class Inventory
    {
        public ObjectId InventoryId { get; set; }

        public InventoryType InventoryType {get; set;}
        public byte[] OwnerId { get; set; }

        public Dictionary<ResourceType, int> OwnerInventory { get; set; }
    }

    public enum InventoryType
    {
        PLAYERINVENTORY,
        TOWNINVENTORY
    }
}

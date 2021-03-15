using DataModel.Common.GameModel;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace DataModel.Server.Model
{
    [BsonIgnoreExtraElements]
    public class Inventory : IUserInventory
    {
        public MongoDB.Bson.ObjectId InventoryId { get; set; }

        public LiteDB.ObjectId InventoryIdLite { get; set; }

        public byte[] OwnerId { get; set; }
        public byte[] ContainerId { get; set; }
        public int StorageCapacity { get; set; }
        public List<DatabaseInventoryStorage> InventoryItems { get; set; }
        Dictionary<InventoryType, int> IUserInventory.InventoryItems { get => InventoryItems.ToInventoryDictionary(); set => InventoryItems = value.ToDatabaseStorage(); }
    }
}

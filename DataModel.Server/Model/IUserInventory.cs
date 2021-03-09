using DataModel.Common.GameModel;
using System.Collections.Generic;

namespace DataModel.Server.Model
{
    public interface IUserInventory
    {
        byte[] OwnerId { get; set; }
        byte[] ContainerId { get; set; }
        int StorageCapacity { get; set; }
        Dictionary<InventoryType, int> InventoryItems { get; set; }
    }
}

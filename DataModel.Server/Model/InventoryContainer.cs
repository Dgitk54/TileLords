using LiteDB;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.Server.Model
{
    public class InventoryContainer
    {
        public ObjectId InventoryContainerId { get; set; }
        public byte[] ContainerId { get; set; }
        public InventoryContainerType ContainerType { get; set; }
    }

    public enum InventoryContainerType
    {
        PLAYER,
        TOWN,
        CARAVAN,
        SECURECHEST
    }
}

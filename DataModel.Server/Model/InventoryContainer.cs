﻿namespace DataModel.Server.Model
{
    public class InventoryContainer
    {
        public byte[] ContainerId { get; set; }
        public InventoryContainerType ContainerType { get; set; }
    }

    public enum InventoryContainerType
    {
        TOWN,
        CARAVAN,
        SECURECHEST
    }
}

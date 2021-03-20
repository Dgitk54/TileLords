using DataModel.Common.Messages;
using MessagePack;
using System;

namespace DataModel.Common.GameModel
{
    [MessagePackObject]
    public class InventoryType : IEquatable<InventoryType>
    {
        [Key(0)]
        public ContentType ContentType { get; set; }
        [Key(1)]
        public ResourceType ResourceType { get; set; }
        public override bool Equals(object obj)
        {
            if (obj is InventoryType)
            {
                var casted = obj as InventoryType;
                return Equals(casted);
            }
            else
            {
                return base.Equals(obj);
            }
        }

        public bool Equals(InventoryType other)
        {
            return other != null && other.ContentType == ContentType && other.ResourceType == ResourceType;
        }

        public override int GetHashCode()
        {
            return Tuple.Create((int)ContentType,(int)ResourceType).GetHashCode();
        }
    }
}

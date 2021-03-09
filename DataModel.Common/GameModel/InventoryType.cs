using DataModel.Common.Messages;
using System;

namespace DataModel.Common.GameModel
{
    public class InventoryType : IEquatable<InventoryType>
    {
        public ContentType ContentType { get; set; }
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

        //TODO: Shift and wrap method? evaluate!
        public override int GetHashCode()
        {
            return Tuple.Create((int)ContentType ^ (int)ResourceType).GetHashCode();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.Common
{



    public class WorldObject : ITileContent
    {


        public WorldObjectType Type { get; set; }

        public WorldObject(WorldObjectType type)
        {
            Type = type;
        }
    }
}

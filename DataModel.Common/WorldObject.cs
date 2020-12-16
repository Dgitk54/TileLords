﻿using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.Common
{



    public class WorldObject : ITileContent
    {

        public int Id { get; set; }        
        public WorldObjectType Type { get; set; }
        public WorldObject() { }
        public WorldObject(WorldObjectType type)
        {
            Type = type;
        }
    }
}
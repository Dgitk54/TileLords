﻿using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.Common
{
    public class MiniTileUpdate : IEvent
    {
        public static readonly string type = "MINITILEUPDATE";
        public MiniTile UpdatedTile { get; set; }
    }
}
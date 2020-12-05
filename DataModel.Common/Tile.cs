﻿using System;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using System.Text;

namespace DataModel.Common
{
    /// <summary>
    /// Class for wrapping up a roughly 275mx275m area in the world.
    /// Made up of MiniTiles.
    /// </summary>
    public class Tile
    {
        public int Id { get; set; }
        public  PlusCode PlusCode { get; set; }
        public  TileType Ttype { get; set; }

        public List<MiniTile> MiniTiles { get; set; }

        public Tile(PlusCode c, TileType t, List<MiniTile> miniTiles) => (PlusCode, Ttype, MiniTiles) = (c, t, miniTiles );

        public Tile() { }
      

    }
}

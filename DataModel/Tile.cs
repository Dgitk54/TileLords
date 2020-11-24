using System;
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

        public  PlusCode Code { get; }
        public  TileType Ttype { get; }

        public List<MiniTile> MiniTiles { get; }

        public Tile(PlusCode c, TileType t, List<MiniTile> miniTiles) => (Code, Ttype, MiniTiles) = (c, t, miniTiles );
      

    }
}

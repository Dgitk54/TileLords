using System;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using System.Text;

namespace DataModel.Common
{
   
    public class Tile
    {

        public  PlusCode Code { get; }
        public  TileType Ttype { get; }

        public List<MiniTile> MiniTiles { get; }

        public Tile(PlusCode c, TileType t, List<MiniTile> miniTiles) => (Code, Ttype, MiniTiles) = (c, t, miniTiles );
      

    }
}

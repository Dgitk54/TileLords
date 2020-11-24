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

        public IReadOnlyList<IReadOnlyList<MiniTile>> MiniTileListList { get; }

        public Tile(PlusCode c, TileType t, List<List<MiniTile>> miniTiles) => (Code, Ttype, MiniTileListList) = (c, t, miniTiles );
      

    }
}

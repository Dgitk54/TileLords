using System;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using System.Text;

namespace DataModel.Common
{
    public enum TileType
    {
       Grassland,
       WaterBody,
       River,
       Mountain,
       Desert,
       Snow,
       Stone,
       Mud
           
    }

    public class Tile
    {

        public  PlusCode code { get; }
        public  TileType ttype { get; }

        public IReadOnlyList<IReadOnlyList<MiniTile>> MiniTileListList { get; }

        public Tile(PlusCode c, TileType t, List<List<MiniTile>> miniTiles) => (code, ttype, MiniTileListList) = (c, t, miniTiles );
      

    }
}

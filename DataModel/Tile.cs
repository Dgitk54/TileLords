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

        public  PlusCode Code { get; }
        public  TileType Ttype { get; }

        public List<MiniTile> MiniTiles { get; }
        public Tile(PlusCode c, TileType t, List<MiniTile> MiniTileContent) => (Code, Ttype, MiniTiles) = (c, t, MiniTileContent);


        public override string ToString() { return "Tile:" + Code.Code; }

    }
}

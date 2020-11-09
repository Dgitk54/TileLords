using System;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using System.Text;

namespace DataModel
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

        public readonly PlusCode code;
        public readonly TileType ttype;
        public readonly Resource activeResource;

        

        public Tile(PlusCode c, TileType t) => (code, ttype) = (c, t);
        public double PlusCode { get; }
        public double TileType { get; }
        public double Resource { get; }

    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.Common
{

    public enum MiniTileType
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

    /// <summary>
    /// Class for wrapping up a roughly 14mx14m area in the world.
    /// </summary>
    public class MiniTile
    {

        public PlusCode Code { get; }
        public MiniTileType TileType { get; }

        public IReadOnlyList<ITileContent> Content { get; }

        public MiniTile(PlusCode c, MiniTileType t, List<ITileContent> con) => (Code, TileType, Content) = (c, t, con);


        public override string ToString() { return "MiniTile:" + Code.Code; }
    }
}

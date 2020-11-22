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

    public class MiniTile
    {



        public MiniTile()
        {

        }




        public PlusCode code { get; }
        public MiniTileType tileType { get; }

        private readonly IReadOnlyList<ITileContent> content;

        public MiniTile(PlusCode c, MiniTileType t, List<ITileContent> con) => (code, tileType, content) = (c, t, con);
        public IReadOnlyList<ITileContent> Content => content;

    }
}

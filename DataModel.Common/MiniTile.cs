using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.Common
{

   

    /// <summary>
    /// Class for wrapping up a roughly 14mx14m area in the world.
    /// </summary>
    public class MiniTile
    {

        public PlusCode MiniTileId { get; set; }
        public MiniTileType TileType { get; set; }

        public IReadOnlyList<ITileContent> Content { get; set; }

        public MiniTile(PlusCode c, MiniTileType t, List<ITileContent> con) => (MiniTileId, TileType, Content) = (c, t, con);


        public override string ToString() { return "MiniTile:" + MiniTileId.Code; }
    }
}

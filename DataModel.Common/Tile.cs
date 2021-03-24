using DataModel.Common.GameModel;
using System.Collections.Generic;

namespace DataModel.Common
{
    /// <summary>
    /// Class for wrapping up a roughly 275mx275m area in the world.
    /// Made up of MiniTiles.
    /// </summary>
    public class Tile
    {
        public PlusCode PlusCode { get; set; }
        public TileType Ttype { get; set; }

        public List<MiniTile> MiniTiles { get; set; }

        public Tile(PlusCode c, TileType t, List<MiniTile> miniTiles)
        {
            (PlusCode, Ttype, MiniTiles) = (c, t, miniTiles);
        }

    }
}

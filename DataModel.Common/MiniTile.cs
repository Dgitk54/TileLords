using DataModel.Common.GameModel;
using System;
using System.Collections.Generic;
namespace DataModel.Common
{



    /// <summary>
    /// Class for wrapping up a roughly 14mx14m area in the world.
    /// </summary>
    public class MiniTile
    {
        public int Id { get; set; }
        public PlusCode MiniTileId { get; set; }
        public MiniTileType TileType { get; set; }

        public TileType ParentType { get; set; }
        public Enum TileTypeAsEnum { get; set; }

        public List<ITileContent> Content { get; set; }

        public MiniTile(PlusCode c, MiniTileType t, List<ITileContent> con)
        {
            (MiniTileId, TileType, Content) = (c, t, con);
        }

        public MiniTile(PlusCode c, MiniTileType t, List<ITileContent> con, TileType p)
        {
            (MiniTileId, TileType, Content, ParentType) = (c, t, con, p);
        }

        public MiniTile(PlusCode c, Enum e, List<ITileContent> con)
        {
            (MiniTileId, TileTypeAsEnum, Content) = (c, e, con);
        }

        public MiniTile() { }
        public override string ToString()
        {


            if (Content != null)
            {
                var ret = "MiniTile:" + MiniTileId.Code + " TileType: " + TileType + " WorldObject: ";
                foreach (ITileContent content in Content)
                {
                    ret = ret + " " + content;
                }
                return ret;

            }
            return "MiniTile:" + MiniTileId.Code + " TileType: " + TileType;
        }
    }
}

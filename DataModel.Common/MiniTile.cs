using System;
using System.Collections.Generic;
using System.Text;
using System.Reactive.Linq;
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

        public IReadOnlyList<ITileContent> Content { get; set; }

        public MiniTile(PlusCode c, MiniTileType t, List<ITileContent> con) => (MiniTileId, TileType, Content) = (c, t, con);

        public MiniTile() { }
        public override string ToString() {


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

using DataModel.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Reactive.Linq;
using System.Linq;

namespace DataModel.Client
{
    public class MapAsRenderAbleChanged : IMessage
    {
        public List<MiniTile> Map { get; set; }
        public PlusCode Location { get; set; }
        public int NullTiles { get; set; }

        public override string ToString()
        {
            var nullcount = 0;
            var nonnull = 0;
            var hasContent = 0;
            string additionalInfo = "";
            foreach (var v in Map)
            {
                if (v == null)
                {
                    nullcount++;
                }
                else
                {
                    if(v.Content != null)
                    {
                        if(v.Content.Count > 0)
                        {
                            hasContent++;

                            v.Content.ToList().ForEach(v2 =>
                            {
                                Player player = null;
                                if (v2 is Player)
                                {
                                    player = v2 as Player;
                                    additionalInfo += "  [Player " + player.Name + "  on:" + v.MiniTileId.Code + "]";
                                }
                            });
                            
                            
                        }
                    }
                    
                    nonnull++;
                }
            }

            return "MapAsRenderAbleChanged "  + "NONNULL:" + nonnull + "TILECONTENT" + hasContent + "    " +  additionalInfo;
        }
    }
}

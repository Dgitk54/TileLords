using DataModel.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Reactive.Linq;

namespace DataModel.Client
{
    public class MapAsRenderAbleChanged : IEvent
    {
        public Dictionary<PlusCode, MiniTile> Map { get; set; }
        public PlusCode Location { get; set; }

        public override string ToString()
        {
            var nullval = Map.Values;
            var nullcount = 0;
            var nonnull = 0;
            var hasContent = 0;
            string additionalInfo = "";
            foreach (var v in nullval)
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
                            if(v.Content.Count == 2)
                            {
                                Player player = null;
                                if(v.Content[1] is Player)
                                {
                                    player = v.Content[1] as Player;
                                    additionalInfo += " and Player " + player.Name;
                                }


                            }
                            
                        }
                    }
                    
                    nonnull++;
                }
            }

            return "MapAsRenderAbleChanged "  + "NONNULL:" + nonnull + "TILECONTENT" + hasContent + "    " +  additionalInfo;
        }
    }
}

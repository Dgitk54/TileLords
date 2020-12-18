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
                        }
                    }
                    
                    nonnull++;
                }
            }

            return "MapAsRenderAbleChanged "  + "NONNULL:" + nonnull + "TILECONTENT" + hasContent;
        }
    }
}

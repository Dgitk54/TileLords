using DataModel.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.Client
{
    public class LatestMapEvent : IEvent
    {
        public IList<MiniTile> TilesToRenderForUnity { get; set; }
        public LatestMapEvent(IList<MiniTile> tiles) => TilesToRenderForUnity = tiles;

        public override string ToString()
        {
            return "LatestMap: Count:" + TilesToRenderForUnity.Count;
        }
    }
}

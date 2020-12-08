using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.Common
{
    public class ServerMapEvent : IEvent
    {

        public int UpdateSize { get; set; }
        
        public IList<Tile> Tiles { get; set; }
        public IList<MiniTile> MiniTiles { get; set; }

    }
}

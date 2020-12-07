using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.Common
{
    public class ServerMapUpdate : IEvent
    {
        public readonly string EventType = "ServerMapUpdate";

        public string UpdateSize { get; set; }
        
        public IList<Tile> Tiles { get; set; }
        public IList<MiniTile> MiniTiles { get; set; }

    }
}

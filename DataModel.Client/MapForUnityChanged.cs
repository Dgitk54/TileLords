using DataModel.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.Client
{
    
    public class MapForUnityChanged : IEvent
    {
        public PlusCode ClientLocation { get; set; }

        public IList<MiniTile> MiniTiles { get; set; }
        public MapForUnityChanged(PlusCode c, IList<MiniTile> miniTiles) => (ClientLocation, MiniTiles) = (c, miniTiles);

        public override string ToString()
        {
            return "MapForUnityEvent: ClientLocation " + ClientLocation.Code + "   MapCount" + MiniTiles.Count;
        }
    }
}

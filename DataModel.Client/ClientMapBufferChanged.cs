using DataModel.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.Client
{
    public class ClientMapBufferChanged : IEvent
    {
        public IList<MiniTile> TilesToRenderForUnity { get; set; }
        public ClientMapBufferChanged(IList<MiniTile> tiles) => TilesToRenderForUnity = tiles;

    }
}

using DataModel.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.Client
{
    public class ClientMapBufferChanged : IMessage
    {
        public Dictionary<PlusCode,MiniTile> TilesToRenderForUnity { get; set; }
        public ClientMapBufferChanged(Dictionary<PlusCode,MiniTile> tiles) => TilesToRenderForUnity = tiles;

        public override string ToString()
        {
            return base.ToString() + "Buffercount: " + TilesToRenderForUnity.Count;
        }

    }
}

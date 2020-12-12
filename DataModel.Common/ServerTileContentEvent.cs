using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.Common
{
    public class ServerTileContentEvent : IEvent
    {
           public Dictionary<PlusCode, ITileContent> VisibleContent { get; set; }
    }
}

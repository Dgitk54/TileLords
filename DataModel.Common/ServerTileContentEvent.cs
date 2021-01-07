using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.Common
{
    public class ServerTileContentEvent : IEvent
    {
           public List<KeyValuePair<PlusCode, List<ITileContent>>> VisibleContent { get; set; }
    }
}

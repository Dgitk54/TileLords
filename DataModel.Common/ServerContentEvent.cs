using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.Common
{
    public class ServerContentEvent : IEvent
    {
        public KeyValuePair<PlusCode, ITileContent> Content { get; set; }
    }
}

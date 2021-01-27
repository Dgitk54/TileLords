using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.Common
{
    public class ServerContentEvent : IMessage
    {
        public KeyValuePair<PlusCode, ITileContent> Content { get; set; }
    }
}

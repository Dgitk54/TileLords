
using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.Common.Messages
{
    public class BatchContentMessage : IMsgPackMsg
    {
        public List<IMsgPackMsg> ContentList { get; set; }
    }
}

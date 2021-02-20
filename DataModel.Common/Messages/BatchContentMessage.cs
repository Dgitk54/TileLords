using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.Common.Messages
{
    [MessagePackObject]
    public class BatchContentMessage : IMsgPackMsg
    {
        [Key(0)]
        public List<IMsgPackMsg> ContentList { get; set; }
    }
}


using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.Common.Messages
{
    public class BatchContentMessage : IMessage
    {
        public List<ContentMessage> ContentList { get; set; }
    }
}

using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.Common.Messages
{
    [MessagePackObject]
    public class UserActionMessage : IMsgPackMsg
    {
        [Key(0)]
        public MessageType MessageType { get; set; }

        [Key(1)]
        public MessageContext MessageContext { get; set; }

        [Key(2)]
        public MessageState MessageState { get; set; }

        [Key(3)]
        public MessageInfo MessageInfo { get; set; }
    }
}

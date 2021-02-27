﻿using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.Common.Messages
{
    public class MapContentTransactionMessage : IMessage
    {
        public MessageType MessageType { get; set; }
        public MessageState MessageState { get; set; }
        public byte[] MapContentId { get; set; }
    }
}

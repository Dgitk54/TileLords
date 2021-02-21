
using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.Common.Messages
{
    
    public class UserActionMessage : IMsgPackMsg
    {
        
        public MessageType MessageType { get; set; }

        
        public MessageContext MessageContext { get; set; }

        
        public MessageState MessageState { get; set; }

        
        public MessageInfo MessageInfo { get; set; }
    }
}

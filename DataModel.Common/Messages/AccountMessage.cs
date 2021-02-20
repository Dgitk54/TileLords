using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.Common.Messages
{
    [MessagePackObject]
    public class AccountMessage : IMsgPackMsg
    {
        [Key(0)]
        public MessageContext Context { get; set; }
        [Key(1)]
        public string Name { get; set; }
        [Key(2)]
        public string Password { get; set; }
    }
}

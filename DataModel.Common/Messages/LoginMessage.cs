using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.Common.Messages
{
    [MessagePackObject]
    public class LoginMessage : IMsgPackMsg
    {
        [Key(0)]
        public string Name { get; set; }

        [Key(1)]
        public string Password { get; set; }

    }
}

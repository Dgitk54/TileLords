
using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.Common.Messages
{
    public class AccountMessage : IMsgPackMsg
    {
        public MessageContext Context { get; set; }
      
        public string Name { get; set; }
       
        public string Password { get; set; }
    }
}

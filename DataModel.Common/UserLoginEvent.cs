using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.Common
{
    public class UserLoginEvent : IMessage
    {
        public string Name { get; set; }
        public string Password { get; set; }
    }
}

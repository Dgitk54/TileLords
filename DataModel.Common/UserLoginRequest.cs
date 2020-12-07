using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.Common
{
    public class UserLoginRequest : IEvent
    {
        public string EventType = "UserLoginRequest";
        public string Name { get; set; }
        public string Password { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.Common
{
    public class UserLoginEvent : IEvent
    {
        public readonly string EventType = "UserLogin";
        public string Name { get; set; }
        public string Password { get; set; }
    }
}

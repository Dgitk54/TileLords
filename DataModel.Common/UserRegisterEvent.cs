using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.Common
{
    public class UserRegisterEvent : IEvent
    {
        public readonly string EventType = "UserRegister";
        public string Name { get; set; }
        public string Password { get; set; }
    }
}

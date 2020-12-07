using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.Common
{
    public class UserRegisterRequest : IEvent
    {
        public readonly string EventType = "UserRegisterRequest";
        public string Name { get; set; }
        public string Password { get; set; }
    }
}

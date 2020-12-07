using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.Common
{
    public class UserLoginEventError : IEvent
    {
        public readonly string EventType = "UserLoginError";
        public string ErrorMessage { get; set; }
    }
}

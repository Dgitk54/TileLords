using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.Common
{
    public class UserRegisterEventError : IEvent
    {
        public readonly string EventType = "UserRegisterError";

        public string ErrorMessage { get; set; }
    }
}

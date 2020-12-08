using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.Common
{
    public class UserLoginEventError : IEvent
    {
        public string ErrorMessage { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.Common
{
    public class UserActionSuccessEvent : IMessage
    {
        public int UserAction { get; set; } 
    }
}

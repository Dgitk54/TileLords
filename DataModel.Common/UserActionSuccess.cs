using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.Common
{
    public class UserActionSuccessEvent : IEvent
    {
        public readonly string EventType = "UserActionSuccess";
        public int UserAction { get; set; } 
    }
}

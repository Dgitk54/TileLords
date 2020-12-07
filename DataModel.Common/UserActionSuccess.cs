using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.Common
{
    public class UserActionSuccessEvent : IEvent
    {
        public string UserAction { get; set; }
    }
}

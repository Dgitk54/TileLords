using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.Common
{
    public class UserActionErrorEvent : IMessage
    {
        public int UserAction { get; set; }
    }
}

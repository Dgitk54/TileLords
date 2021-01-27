using DataModel.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.Server
{
    public class PlayerVisibleEvent : IEvent
    {
        public Player VisiblePlayer {get; set;}
    }
}

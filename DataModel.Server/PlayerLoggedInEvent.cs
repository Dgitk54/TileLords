using DataModel.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.Server
{
    public class PlayerLoggedInEvent : IEvent
    {
        public ObservablePlayer Player { get; set; }

    }
}

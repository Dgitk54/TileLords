using DataModel.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.Server
{
    public class PlayersOnlineEvent : IEvent
    {
        public IReadOnlyList<ObservablePlayer> PlayersOnline { get; set; }
    }
}

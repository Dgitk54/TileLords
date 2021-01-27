using DataModel.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.Server
{
    public class PlayersOnlineEvent : IMessage
    {
        public IReadOnlyList<ObservablePlayer> PlayersOnline { get; set; }
    }
}

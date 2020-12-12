using DataModel.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Reactive.Linq;

namespace DataModel.Server
{
    public class ClientToClientPositionUpdateHandler
    {

        readonly IEventBus serverBus;
        public ClientToClientPositionUpdateHandler(IEventBus serverBus)
        {
            this.serverBus = serverBus;
        }

        public IDisposable AttachToBus()
        {

            var players = from e in serverBus.GetEventStream<PlayerLoggedInEvent>()
                          select e.Player;

            var closedChannels = from e in serverBus.GetEventStream<ServerClientDisconnectedEvent>()
                                 select e.Channel;

            var combined = players.CombineLatest(closedChannels, (player, channel) => new { player, channel })
                   .Scan(new List<ObservablePlayer>(), (list, merged) =>
            {
                if (!list.Contains(merged.player))
                    list.Add(merged.player);

                list.RemoveAll(v => v.ClientChannel.Id.CompareTo(merged.channel.Id) == 0);

                return list;
            });



            //TODO: this wont scale well...
            var playersClose = from list in combined
                               from p1 in list
                               from p2 in list
                               where p1 != p2
                               from pos1 in p1.CurrentPosition
                               from pos2 in p2.CurrentPosition
                               where pos1.GetChebyshevDistance(pos2) < 50
                               select new { p1, p2 };




            return playersClose.Subscribe(v =>
            {

            });



        }




    }
}

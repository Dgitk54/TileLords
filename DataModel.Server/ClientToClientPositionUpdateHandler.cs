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



            var players = from e in serverBus.GetEventStream<PlayerLoggedInEvent>().StartWith(new PlayerLoggedInEvent())
                          select e.Player;



            var closedChannels = from e in serverBus.GetEventStream<ServerClientDisconnectedEvent>().StartWith(new ServerClientDisconnectedEvent())
                                 select e.Channel;


            var eventsLatestFrom = from player in players
                             from channel in closedChannels
                             select (player, channel);



            //var eventsLatestFrom = players.CombineLatest(closedChannels, (player, channel) => new { player, channel });


            //TODO: after disconnecting scan stays at 1 player
            var playersOnline = eventsLatestFrom
                                       .Scan(new List<ObservablePlayer>(), (list, merged) =>
            {
                if(merged.player != null)
                {
                    if (!list.Contains(merged.player) && merged.player.ClientChannel.Active)
                        list.Add(merged.player);
                }
                

                if (merged.channel != null)
                    list.RemoveAll(v => v.ClientChannel.Equals(merged.channel));
                
                Console.WriteLine("Players Online:" + list.Count);

                return list;
            });




            //TODO: this wont scale well...
            var playersClose = from list in playersOnline
                               from p1 in list
                               from p2 in list
                               where p1 != p2
                               from pos1 in p1.PlayerObservableLocationStream
                               from pos2 in p2.PlayerObservableLocationStream
                               where pos1.GetChebyshevDistance(pos2) < 50
                               select new { p1, pos1, p2, pos2 };

            return playersClose.Subscribe(v =>
            {
                var player1AsTileContent = new Player() { Name = v.p1.Name, Location = v.pos1 };
                var player2AsTileContent = new Player() { Name = v.p2.Name, Location = v.pos2 };


                var dictForP1 = new Dictionary<PlusCode, ITileContent>()
                {
                    {v.pos2, player2AsTileContent }
                };
                var dictForP2 = new Dictionary<PlusCode, ITileContent>()
                {
                    {v.pos1, player1AsTileContent }
                };

                var contentForPlayer1 = new ServerTileContentEvent() { VisibleContent = dictForP1 };
                var contentForPlayer2 = new ServerTileContentEvent() { VisibleContent = dictForP2 };

                v.p1.ClientBus.Publish(contentForPlayer1);
                v.p2.ClientBus.Publish(contentForPlayer2);
            });



        }




    }
}

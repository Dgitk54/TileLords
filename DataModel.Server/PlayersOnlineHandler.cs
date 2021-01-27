using DataModel.Common;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;

namespace DataModel.Server
{
    public class PlayersOnlineHandler
    {

        readonly IMessageBus eventBus;
        public PlayersOnlineHandler(IMessageBus serverBus)
        {
            eventBus = serverBus;
        }

        public IDisposable AttachToBus()
        {


            var playerLogInStream = eventBus.GetEventStream<PlayerLoggedInEvent>();

            
            
            var disconnectedPlayers = from e in playerLogInStream
                                      from v in e.Player.ConnectionStatus
                                      where v == false
                                      select e.Player;

            var connectedPlayers = from e in playerLogInStream
                                   select e.Player;

            var connectedAndDisconnected = connectedPlayers.Merge(disconnectedPlayers);

       


            var activeOnly = connectedAndDisconnected.Scan(new List<ObservablePlayer>(), (list, player) =>
            {
                if (player == null)
                    return list;

                if (list.Contains(player))
                {
                    list.Remove(player);
                    return list;
                } else
                {
                    list.Add(player);
                    return list;
                }

            });

           

            return activeOnly.Subscribe(v =>
            {
                eventBus.Publish(new PlayersOnlineEvent()
                {
                    PlayersOnline = v
                });
            }
            );
        }
    }
}

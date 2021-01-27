using DataModel.Common;
using Google.OpenLocationCode;
using LiteDB;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading;

namespace DataModel.Server
{
    public class PlayerToPlayerHandler
    {
        readonly IEventBus serverBus;
        public PlayerToPlayerHandler(IEventBus eventBus)
        {
            serverBus = eventBus;
        }
        public IDisposable AttachToBus()
        {
            var allOnlinePlayers = serverBus.GetEventStream<PlayersOnlineEvent>();


            var eachPlayer = from playerEvent in allOnlinePlayers
                             from player in playerEvent.PlayersOnline
                             from location in player.PlayerObservableLocationStream
                             select (player, location, playerEvent.PlayersOnline);

            var toSend = from touple in eachPlayer
                         from otherPlayers in touple.PlayersOnline
                         where touple.player.Id != otherPlayers.Id
                         from otherPlayerLocation in otherPlayers.PlayerObservableLocationStream
                         where touple.location.GetChebyshevDistance(otherPlayerLocation) < 40
                         select (touple.player, otherPlayers, otherPlayerLocation);

            return toSend.DistinctUntilChanged()
                         .Do(v => Console.WriteLine("Pushing for" + v.player.Name + "     Content: " + v.otherPlayers.Name + "  " + v.otherPlayerLocation))
                         .Subscribe(v =>
                         {
                             v.player.ClientBus.Publish(new PlayerVisibleEvent() { VisiblePlayer = new Player() { Id = v.otherPlayers.Id, Location = v.otherPlayerLocation, Name = v.otherPlayers.Name } });
                         });

        }


    }
}

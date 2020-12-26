using DataModel.Common;
using Google.OpenLocationCode;
using LiteDB;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Text;

namespace DataModel.Server
{
    public class PlayerMovementTileUpdater
    {
        readonly IEventBus eventBus;

        public PlayerMovementTileUpdater(IEventBus serverBus)
        {
            eventBus = serverBus;
        }

        public IDisposable AttachCleanup()
        {
            var playerLogInStream = eventBus.GetEventStream<PlayerLoggedInEvent>();



            var disconnectedPlayers = from e in playerLogInStream
                                      from lastPosition in e.Player.PlayerObservableLocationStream
                                      from v in e.Player.ConnectionStatus
                                      where v == false
                                      select (e.Player, lastPosition);

            return disconnectedPlayers.Subscribe(v =>
            {
                var tile = DataBaseFunctions.LookupMiniTile(v.lastPosition);
                var player = new Player { Location = v.lastPosition, Name = v.Player.Name };
                RemovePlayer(tile, player);
                var toPublish = new ServerMapEvent() { MiniTiles = new List<MiniTile>() { tile } };
                eventBus.Publish(toPublish);
            });
        }
        public IDisposable AttachToBus()
        {
            var allOnlinePlayers = eventBus.GetEventStream<PlayersOnlineEvent>();

            var playerMovement = from playerEvent in allOnlinePlayers
                                 from player in playerEvent.PlayersOnline
                                 select (player, PairWithPrevious(player.PlayerObservableLocationStream.DistinctUntilChanged()));

            var movementOnly = from touple in playerMovement
                               from posChange in touple.Item2
                               select (touple.player, posChange);


            

            return movementOnly.Subscribe(v =>
             {
                 //Client just logged in, and hasn't moved yet
                 if (v.posChange.Item2.Equals(default(PlusCode)))
                 {
                     //Only fire content event

                     var tile = DataBaseFunctions.LookupMiniTile(v.posChange.Item1);
                     var player = new Player { Location = tile.MiniTileId, Name = v.player.Name };
                     AddPlayer(tile, player);
                     var toPublish = new ServerMapEvent() { MiniTiles = new List<MiniTile>() { tile } };
                     eventBus.Publish(toPublish);
                 }
                 else
                 {
                     var tileOld = DataBaseFunctions.LookupMiniTile(v.posChange.Item2);
                     var tileNew = DataBaseFunctions.LookupMiniTile(v.posChange.Item1);

                     var playerOld = new Player { Location = tileOld.MiniTileId, Name = v.player.Name };
                     var playerNew = new Player { Location = tileNew.MiniTileId, Name = v.player.Name };
                     RemovePlayer(tileOld, playerOld);
                     AddPlayer(tileNew, playerNew);

                     var toPublish = new ServerMapEvent() { MiniTiles = new List<MiniTile>() { tileOld, tileNew } };
                     eventBus.Publish(toPublish);

                 }
             });
        }
        static void RemovePlayer(MiniTile tile, Player player)
        {
            var newContent = new List<ITileContent>(tile.Content);
            var removed = newContent.Remove(player);
            Debug.Assert(removed);
            tile.Content = newContent;

        }


        static void AddPlayer(MiniTile tile, Player player)
        {
            var newContent = new List<ITileContent>(tile.Content);
            newContent.Add(player);
            tile.Content = newContent;

        }

        public static IObservable<Tuple<TSource, TSource>> PairWithPrevious<TSource>(IObservable<TSource> source)
        {
            return source.Scan(
                Tuple.Create(default(TSource), default(TSource)),
                (acc, current) => Tuple.Create(acc.Item2, current));
        }
    }
}

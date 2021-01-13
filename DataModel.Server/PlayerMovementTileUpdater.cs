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
    /// <summary>
    /// Serverwide handler responsible for updating the tiles players are standing on / moving to.
    /// </summary>
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



            var playerDisconnectedWithLocation = from e in playerLogInStream
                                                 from merged in e.Player.ConnectionStatus.WithLatestFrom(e.Player.PlayerObservableLocationStream, (val1, lastPosition) => new { val1, lastPosition })
                                                 where merged.val1 == false
                                                 select (e.Player, merged);

            return playerDisconnectedWithLocation.Subscribe(v =>
            {
                GetWithRetries(v.merged.lastPosition)
                .Subscribe(tile =>
                {
                    var player = new Player { Location = v.merged.lastPosition, Name = v.Player.Name };
                    RemovePlayer(tile, player);
                    var toPublish = new ServerMapEvent() { MiniTiles = new List<MiniTile>() { tile } };
                    eventBus.Publish(toPublish);
                });
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





            return movementOnly.DistinctUntilChanged().Subscribe(v =>
             {
                 //Client just logged in, and hasn't moved yet
                 if (v.posChange.Item1.Equals(default(PlusCode)))
                 {
                     //Only fire content event
                     GetWithRetries(v.posChange.Item2)
                     .Where(tile => tile != null)
                     .Subscribe(t =>
                     {
                         var player = new Player { Location = t.MiniTileId, Name = v.player.Name };
                         AddPlayer(t, player);
                         var toPublish = new ServerMapEvent() { MiniTiles = new List<MiniTile>() { t } };
                         eventBus.Publish(toPublish);
                     }, e =>
                     {
                         Console.WriteLine("Could not get single tile for" + v.posChange.Item2.Code);
                     });
                 }
                 else
                 {

                     GetWithRetries(v.posChange.Item1)
                     .WithLatestFrom(GetWithRetries(v.posChange.Item2), (tileOld, tileNew) => new { tileOld, tileNew })
                     .Subscribe(t =>
                     {
                         var playerOld = new Player { Location = t.tileOld.MiniTileId, Name = v.player.Name };
                         var playerNew = new Player { Location = t.tileNew.MiniTileId, Name = v.player.Name };
                         RemovePlayer(t.tileOld, playerOld);
                         AddPlayer(t.tileNew, playerNew);

                         var toPublish = new ServerMapEvent() { MiniTiles = new List<MiniTile>() { t.tileOld, t.tileNew } };
                         eventBus.Publish(toPublish);
                     }, e =>
                     {
                         Console.WriteLine("Could not get double tile for" + v.posChange.Item2.Code + "  " + v.posChange.Item1.Code);
                     });

                 }
             });
        }
        static void RemovePlayer(MiniTile tile, Player player)
        {
            var newContent = new List<ITileContent>(tile.Content);


            var samePlayerCount = newContent.Where(v =>
            {
                if (v is Player)
                {
                    var tmp = v as Player;
                    if (tmp.Name.Equals(player.Name))
                    {
                        return true;
                    }
                }
                return false;
            }).Count();

            if (samePlayerCount > 0)
            {

                var samePlayer = newContent.Where(v =>
                {
                    if (v is Player)
                    {
                        var tmp = v as Player;
                        if (tmp.Name.Equals(player.Name))
                        {
                            return true;
                        }
                    }
                    return false;
                }).First();

                var removed = newContent.Remove(samePlayer);

                tile.Content = newContent;

            }



        }




        static void AddPlayer(MiniTile tile, Player player)
        {
            var newContent = new List<ITileContent>(tile.Content);
            newContent.Add(player);
            tile.Content = newContent;

        }

        static IObservable<Tuple<TSource, TSource>> PairWithPrevious<TSource>(IObservable<TSource> source)
        {
            return source.Scan(
                Tuple.Create(default(TSource), default(TSource)),
                (acc, current) => Tuple.Create(acc.Item2, current));
        }


        static IObservable<MiniTile> GetWithRetries(PlusCode tileCode)
        {
            var source = Observable.Defer(() => LookupObservable(tileCode));

            int attempt = 0;
            return Observable.Defer(() =>
            {
                return ((++attempt == 1) ? source : source.DelaySubscription(TimeSpan.FromMilliseconds(3000)));
            })
                .Retry(4);

        }


        static IObservable<MiniTile> LookupObservable(PlusCode tile)
        {
            return Observable.Create<MiniTile>(v =>
            {
                var lookUp = DataBaseFunctions.LookupOnlyMiniTile(tile);
                if (lookUp == null)
                    v.OnError(new Exception("Tile not found"));

                v.OnNext(DataBaseFunctions.LookupOnlyMiniTile(tile));
                v.OnCompleted();
                return Disposable.Empty;
            });

        }
    }
}

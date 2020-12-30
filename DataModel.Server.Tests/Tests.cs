using DataModel.Common;
using DataModel.Server;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace ClientIntegration
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
            if (File.Exists(@"MyData.db"))
                File.Delete(@"MyData.db");

            var lookupGenerate = DataBaseFunctions.LookUpWithGenerateTile(new PlusCode("8FX9XW2F+", 8));
        }

        [TearDown]
        public void TearDown()
        {
            if (File.Exists(@"MyData.db"))
            {
                File.Delete(@"MyData.db");
            }
        }

        [Test]
        public void GivenPlusCodeNeighborsIn8Works()
        {
            var code = new PlusCode("8FX9WWV9+", 8);
            var outcome = ServerFunctions.NeighborsIn8(code);

            Assert.IsTrue(outcome.Count == 9);
            Assert.IsTrue(outcome.Contains(code));
        }


        [Test]
        public void PlayerOnlineHandlerFunctionsProperly()
        {
            var p1State = new Subject<bool>();
            var p2State = new Subject<bool>();
            var p3State = new Subject<bool>();

            var player1 = new ObservablePlayer()
            {
                Name = "Player1",
                ConnectionStatus = p1State
            };
            var player2 = new ObservablePlayer()
            {
                Name = "Player2",
                ConnectionStatus = p2State
            };
            var player3 = new ObservablePlayer()
            {
                Name = "Player3",
                ConnectionStatus = p3State
            };


            var eventBus = new ServerEventBus();
            var handler = new PlayersOnlineHandler(eventBus);

            handler.AttachToBus();

            int onlinePlayers = 0;


            eventBus.GetEventStream<PlayersOnlineEvent>().Subscribe(v => { onlinePlayers = v.PlayersOnline.Count; });
            eventBus.Publish(new PlayerLoggedInEvent() { Player = player1 });



            Assert.IsTrue(onlinePlayers == 1);

            eventBus.Publish(new PlayerLoggedInEvent() { Player = player2 });

            Assert.IsTrue(onlinePlayers == 2);

            p1State.OnNext(false);

            Assert.IsTrue(onlinePlayers == 1);

            p2State.OnNext(false);

            Assert.IsTrue(onlinePlayers == 0);

        }

        [Test]
        public void PlayerMovementTileUpdaterFiresServerEventsOnMovement()
        {
            var p1State = new Subject<bool>();
            var p1Gps = new Subject<PlusCode>();
            var player1 = new ObservablePlayer()
            {
                Name = "Player1",
                ConnectionStatus = p1State,
                PlayerObservableLocationStream = p1Gps.AsObservable()

            };
            var playerOnlineList = new List<ObservablePlayer>() { player1 };
            var cleanUp = new List<IDisposable>();

            var loggedInForCleanup = new PlayerLoggedInEvent() { Player = player1 };
            var playerOnline = new PlayersOnlineEvent() { PlayersOnline = playerOnlineList.AsReadOnly() };
            var eventBus = new ServerEventBus();


            var movementHandler = new PlayerMovementTileUpdater(eventBus);


            var plus1 = new PlusCode("8FX9XW2F+9X", 10);
            var plus2 = new PlusCode("8FX9XW2F+8X", 10);

            cleanUp.Add(movementHandler.AttachToBus());
            cleanUp.Add(movementHandler.AttachCleanup());






            int changedTiles = 0;
            int eventsFired = 0;
            List<MiniTile> tilesChanged = null;
            eventBus.GetEventStream<ServerMapEvent>().Subscribe(v =>
            {
                changedTiles = v.MiniTiles.Count;
                tilesChanged = new List<MiniTile>(v.MiniTiles);
                eventsFired++;
            });

            eventBus.Publish(loggedInForCleanup);
            eventBus.Publish(playerOnline);


            Assert.IsTrue(changedTiles == 0);
            Assert.IsTrue(eventsFired == 0);
            //Spawn player on tile
            p1Gps.OnNext(plus1);
            //player has not moved yet, only one tile should be affected
            Assert.IsTrue(changedTiles == 1);
            Assert.IsTrue(eventsFired == 1);
            //Move player
            p1Gps.OnNext(plus2);
            
            //2 tiles have been affected (one where player is no longer standing on, one where the player is standing on now, changedTiles should be 2)
            Assert.IsTrue(changedTiles == 2);
            Assert.IsTrue(tilesChanged.Count == 2);
            Assert.IsTrue(eventsFired == 2);




            //Assure the affected tiles have a playerTileContent on them:
            var playerTileEnumerable = from e in tilesChanged
                                       where e.MiniTileId.Equals(plus2)
                                       select e;
            var playerTileLocation = playerTileEnumerable.First();

            Assert.IsTrue(playerTileLocation != null);
            var playerTileContent = playerTileLocation.Content;

            var onTile = from e in playerTileContent
                         where e is Player
                         select e;


            //Assure the tile the player is standing on has a player tilecontent
            Assert.IsTrue(onTile.Count() == 1);




            //Check tile player should not be on
            var emptyTile = from e in tilesChanged
                            where e.MiniTileId.Equals(plus1)
                            select e;
            var emptyMiniTile = emptyTile.First();
            Assert.IsTrue(emptyMiniTile != null);


            var notOnTile = from e in emptyMiniTile.Content
                            where e is Player
                            select e;

            Assert.IsTrue(notOnTile.Count() == 0);





            //Test cleanup: Player disappears from tile he is standing on if he disconnects:

            p1State.OnNext(false);
            Assert.IsTrue(eventsFired == 3);
            Assert.IsTrue(changedTiles == 1);
            Assert.IsTrue(tilesChanged.Count == 1);

            var tile = tilesChanged[0];

            Assert.IsTrue(tile.MiniTileId.Equals(plus2));
            var playerDespawned = from e in tile.Content
                                  where e is Player
                                  select e;

            Assert.IsTrue(notOnTile.Count() == 0);






            //Cleanup
            cleanUp.ForEach(v => v.Dispose());



        }




    }
}
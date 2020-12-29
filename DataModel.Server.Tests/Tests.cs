using DataModel.Common;
using DataModel.Server;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
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

            
            eventBus.GetEventStream<PlayersOnlineEvent>().Subscribe(v => { onlinePlayers = v.PlayersOnline.Count;  });
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
        public void PlayerMovementTileUpdaterFiresEventsOnMovement()
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

            var playerOnline = new PlayersOnlineEvent() { PlayersOnline = playerOnlineList.AsReadOnly() };
            var eventBus = new ServerEventBus();
            var movementHandler = new PlayerMovementTileUpdater(eventBus);


            var plus1 = new PlusCode("8FX9XW2F+9X", 10);
            var plus2 = new PlusCode("8FX9XW2F+8X", 10);

            cleanUp.Add(movementHandler.AttachToBus());
            cleanUp.Add(movementHandler.AttachCleanup());

            int changedTiles = 0;
            List<MiniTile> tilesChanged = null;
            eventBus.GetEventStream<ServerMapEvent>().Subscribe(v =>
            {
                changedTiles = v.MiniTiles.Count;
                tilesChanged = new List<MiniTile>(v.MiniTiles);
            });
            eventBus.Publish(playerOnline);
            Assert.IsTrue(changedTiles == 0);

            p1Gps.OnNext(plus1);

            Assert.IsTrue(changedTiles == 1);

            p1Gps.OnNext(plus2);

            Assert.IsTrue(changedTiles == 2);
            Assert.IsTrue(tilesChanged.Count == 2);

            //Check tile player is on
            var playerTileEnumerable = from e in tilesChanged
                                 where e.MiniTileId.Equals(plus2)
                                 select e;
            var playerTileLocation = playerTileEnumerable.First();
            Assert.IsTrue(playerTileLocation != null);
            var playerTileContent = playerTileLocation.Content;

            var onTile = from e in playerTileContent
                         where e is Player
                         select e;

            Assert.IsTrue(onTile.Count() == 1);
            ;



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



            cleanUp.ForEach(v => v.Dispose());


        }
    }
}
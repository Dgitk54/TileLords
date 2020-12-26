using DataModel.Common;
using DataModel.Server;
using NUnit.Framework;
using System;
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
    }
}
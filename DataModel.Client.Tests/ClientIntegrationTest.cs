using DataModel.Client;
using DataModel.Common;
using DataModel.Server;
using NUnit.Framework;
using System.Threading.Tasks;
using System.Reactive.Linq;
using System;

namespace ClientIntegration
{
    public class ClientIntegrationTest
    {
        private ServerInstance server;
        private Task serverRunning;


        static async Task<(IEventBus, ClientInstance, Task)> StartClient()
        {
            var bus = new ClientEventBus();
            var instance = new ClientInstance(bus);

            var waitForConnection = Task<ClientConnectedEvent>.Factory.StartNew(() =>
            {
                ClientConnectedEvent result = bus.GetEventStream<ClientConnectedEvent>().Take(1)
                            .Timeout(DateTime.Now.AddSeconds(5)).Wait();
                return result;
            });


            var startServer = Task.Factory.StartNew(() => instance.RunClientAsync());
            await waitForConnection;
            await startServer;
            
            Assert.IsTrue(waitForConnection.Result != null);
            
            return (bus, instance, startServer);
        }

        static async Task GetsEvent<T, T2>(ClientInstance instance, T2 input, int timeOutInSeconds) where T : IEvent where T2 : IEvent
        {

            var received = Task<T>.Factory.StartNew(() =>
            {
                var result = instance.EventBus.GetEventStream<T>().Take(1).Wait();
                return result;
            });

            var send = Task.Factory.StartNew(() => { instance.EventBus.Publish(input); } );

            Task.WaitAll(received, send);
            Assert.IsTrue(received.Result != null);
        }


        [SetUp]
        public void StartServer()
        {
            server = new ServerInstance();
            serverRunning = server.RunServerAsync();


        }

        [Test]
        public void CanConnect()
        {
            var result = StartClient();
            result.Wait();
            var instance = result.Result.Item2;
            instance.DisconnectClient();
            result.Result.Item3.Dispose();
        }


        [Test]
        public void GetsNegativeResponseToRegisterAccount()
        {
            var result = StartClient();
            var instance = result.Result.Item2;
            var bus = result.Result.Item1;
            var testLogin = GetsEvent<UserActionErrorEvent, UserRegisterEvent>(instance, new UserRegisterEvent() { Name = "a", Password = "a" }, 5);

            instance.DisconnectClient();


        }


        [Test]
        public void GetsMapResponseToGpsSend()
        {
            var result = StartClient();

            result.Wait();
            var instance = result.Result.Item2;
            var bus = result.Result.Item1;

            GetsEvent<UserActionSuccessEvent, UserLoginEvent>(instance, new UserLoginEvent() { Name = "a", Password = "a" }, 10).Wait();
            GetsEvent<MapAsRenderAbleChanged, UserGpsEvent>(instance, new UserGpsEvent(new GPS(49.000, 30.000)), 10).Wait();


            instance.DisconnectClient();
            result.Result.Item3.Dispose();
        }





        [Test]
        public void GetsResponseToLoginEvent()
        {

            var result = StartClient();
            var instance = result.Result.Item2;
            var bus = result.Result.Item1;


            var testLogin = GetsEvent<UserActionSuccessEvent, UserLoginEvent>(instance, new UserLoginEvent() { Name = "a", Password = "a" }, 5);

            instance.DisconnectClient();
        }

        [TearDown]
        public void ShutDown()
        {
            server.ShutDownServer().Wait();
        }

    }
}
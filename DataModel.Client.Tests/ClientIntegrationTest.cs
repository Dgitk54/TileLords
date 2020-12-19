using DataModel.Client;
using DataModel.Common;
using DataModel.Server;
using NUnit.Framework;
using System.Threading.Tasks;
using System.Reactive.Linq;
using System;
using System.Threading;
using System.Reactive.Concurrency;
using System.Collections.Generic;
using System.Diagnostics;

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
            var observeOn = Scheduler.CurrentThread;
            //.ObserveOn(observeOn)
            ClientFunctions.DebugEventsToDebugSink(instance.EventBus.GetEventStream<IEvent>());


            var waitForConnection = Task.Run(() =>
            {
                ClientConnectedEvent result = bus.GetEventStream<ClientConnectedEvent>().Take(1)
                            .Timeout(DateTime.Now.AddSeconds(5)).Wait();
                return result;
            });
            Thread.Sleep(300);
            var startClient = Task.Run(instance.RunClientAsync);

            await waitForConnection;



            Assert.IsTrue(waitForConnection.Result != null);
            ;
            return (bus, instance, startClient);
        }


        static async Task<T> GetsEvent<T, T2>(ClientInstance instance, T2 input, int timeOutInSeconds) where T : IEvent where T2 : IEvent
        {
            var observeOn = Scheduler.CurrentThread;

            var received = Task.Run(() =>
            {
                var result = instance.EventBus.GetEventStream<T>().Take(1).Timeout(DateTime.Now.AddSeconds(timeOutInSeconds)).ObserveOn(observeOn).Wait();
                return result;
            });
            Thread.Sleep(200);

            var publish = Task.Run(() => instance.SendTyped(input));
            await received;
            await publish;
            ;
            Assert.IsTrue(received.Result != null);
            return received.Result;
        }


        static async Task<T> GetsEvent<T>(ClientInstance instance, int timeOutInSeconds) where T : IEvent
        {
            var observeOn = Scheduler.CurrentThread;
            var received = Task.Run(() =>
            {
                var result = instance.EventBus.GetEventStream<T>().Take(1).Timeout(DateTime.Now.AddSeconds(timeOutInSeconds)).ObserveOn(observeOn).Wait();
                return result;
            });
            Thread.Sleep(200);
            await received;

            return received.Result;

        }
        static void SendDebugGps(ClientInstance instance, CancellationToken ct, int iterations, int sleeptime)
        {

            var start = new GPS(49.000000, 7.900000);
            double step = 0.000150;
            var list = DataModelFunctions.GPSNodesWithOffsets(start, step, step, 60);

            for (int i = 0; i < iterations; i++)
            {
                instance.SendDebugGPS(list[i]);
                Thread.Sleep(sleeptime);
                if (ct.IsCancellationRequested)
                    break;
            }
        }

        static void SendGpsPath(ClientInstance instance, CancellationToken ct, List<GPS> gps, int sleeptime)
        {
            int i = 0;
            do
            {
                instance.SendDebugGPS(gps[i % gps.Count]);
                Thread.Sleep(sleeptime);
                i++;
                if (ct.IsCancellationRequested)
                    break;

            } while (!ct.IsCancellationRequested);


        }


        static void DebugLoginSendSameGps(string name, string password, GPS gpsToSend, CancellationToken cancellationToken)
        {
            var result = StartClient();
            result.Wait();
            var instance = result.Result.Item2;
            var nodesAmount = 20;
            var tokenSrc = new CancellationTokenSource();
            var list = DataModelFunctions.GPSNodesInCircle(gpsToSend, nodesAmount, 0.001);


            //Try to log in, create account if cant log in:

            Task tryLogin;
            try
            {
                tryLogin = GetsEvent<UserActionSuccessEvent, UserLoginEvent>(instance, new UserLoginEvent() { Name = name, Password = password }, 5);
                tryLogin.Wait();
            }
            catch (AggregateException exception)
            {
                var tryRegister = GetsEvent<UserActionSuccessEvent, UserRegisterEvent>(instance, new UserRegisterEvent() { Name = name, Password = password }, 5);
                tryRegister.Wait();
                if (tryRegister.IsFaulted)
                {
                    throw new Exception("Can not log in nor register");
                }

                var tryLoginAfterRegister = GetsEvent<UserActionSuccessEvent, UserLoginEvent>(instance, new UserLoginEvent() { Name = name, Password = password }, 5);
                tryLoginAfterRegister.Wait();
                if (tryLoginAfterRegister.IsFaulted)
                    throw new Exception("Can not log in after register");
            }

            //var runCircle = Task.Run(() => SendGpsPath(instance, tokenSrc.Token, list, 4000), tokenSrc.Token);

            var sendSame = Task.Run(() => SendSameGps(instance, tokenSrc.Token, gpsToSend, 4000));

            do
            {
                if (cancellationToken.IsCancellationRequested)
                    break;
                Thread.Sleep(1000);

            } while (!cancellationToken.IsCancellationRequested);

            tokenSrc.Cancel();
            sendSame.Wait();
            instance.DisconnectClient();
        }

        static void DebugLoginAndRunAroundClient(string name, string password, GPS circleCenter, CancellationToken cancellationToken)
        {
            var result = StartClient();
            result.Wait();
            var instance = result.Result.Item2;
            var nodesAmount = 20;
            var tokenSrc = new CancellationTokenSource();
            var list = DataModelFunctions.GPSNodesInCircle(circleCenter, nodesAmount, 0.001);


            //Try to log in, create account if cant log in:

            Task tryLogin;
            try
            {
                tryLogin = GetsEvent<UserActionSuccessEvent, UserLoginEvent>(instance, new UserLoginEvent() { Name = name, Password = password }, 5);
                tryLogin.Wait();
            }
            catch (AggregateException exception)
            {
                var tryRegister = GetsEvent<UserActionSuccessEvent, UserRegisterEvent>(instance, new UserRegisterEvent() { Name = name, Password = password }, 5);
                tryRegister.Wait();
                if (tryRegister.IsFaulted)
                {
                    throw new Exception("Can not log in nor register");
                }
         
                var tryLoginAfterRegister = GetsEvent<UserActionSuccessEvent, UserLoginEvent>(instance, new UserLoginEvent() { Name = name, Password = password }, 5);
                tryLoginAfterRegister.Wait();
                if (tryLoginAfterRegister.IsFaulted)
                    throw new Exception("Can not log in after register");
            }
            
            var runCircle = Task.Run(() => SendGpsPath(instance, tokenSrc.Token, list, 4000), tokenSrc.Token);
            do
            {
                if (cancellationToken.IsCancellationRequested)
                    break;
                Thread.Sleep(1000);

            } while (!cancellationToken.IsCancellationRequested);

            tokenSrc.Cancel();
            runCircle.Wait();
            instance.DisconnectClient();
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
            var instance = result.Result.Item2;
            instance.DisconnectClient();
        }


        [Test]
        public void TwoClientsCanConnectAndLogIn()
        {
            var token = new CancellationTokenSource();
            var client1 = Task.Run(() => DebugLoginSendSameGps("a", "a", new GPS(49.000000, 7.900000), token.Token));
            var client2 = Task.Run(() => DebugLoginSendSameGps("b", "b", new GPS(49.000005, 7.900005), token.Token));
            Thread.Sleep(60 * 1000);
            token.Cancel();
            client1.Wait();
            //client2.Wait();

        }
        [Test]
        public void GetsNegativeResponseToRegisterAccount()
        {
            var result = StartClient();
            result.Wait();
            var instance = result.Result.Item2;
            GetsEvent<UserActionErrorEvent, UserRegisterEvent>(instance, new UserRegisterEvent() { Name = "a", Password = "a" }, 5).Wait();
            instance.DisconnectClient();
        }

        [Test]
        public void GetsMapBufferChangeAfterGPS()
        {
            var result = StartClient();
            result.Wait();

            var instance = result.Result.Item2;
            CancellationTokenSource tokenSrc = new CancellationTokenSource();

            var cleanUp = Task.Run(() => SendDebugGps(instance, tokenSrc.Token, 5, 1000), tokenSrc.Token);
            GetsEvent<ClientMapBufferChanged, UserGpsEvent>(instance, new UserGpsEvent(new GPS(49.000000, 7.900000)), 10).Wait();
            tokenSrc.Cancel();
            cleanUp.Wait();
            instance.DisconnectClient();
        }




        [Test]
        public void GetsMapResponseToGpsSendAfterLogin()
        {
            var result = StartClient();

            result.Wait();
            var instance = result.Result.Item2;


            GetsEvent<UserActionSuccessEvent, UserLoginEvent>(instance, new UserLoginEvent() { Name = "a", Password = "a" }, 10).Wait();

            CancellationTokenSource tokenSrc = new CancellationTokenSource();
            var cleanUp = Task.Run(() => SendDebugGps(instance, tokenSrc.Token, 20, 2000), tokenSrc.Token);



            GetsEvent<MapAsRenderAbleChanged, UserGpsEvent>(instance, new UserGpsEvent(new GPS(49.000000, 7.900000)), 20).Wait();
            Stopwatch watch = Stopwatch.StartNew();

            tokenSrc.Cancel();
            cleanUp.Wait();
            instance.DisconnectClient();
        }





        [Test]
        public void GetsResponseToLoginEvent()
        {

            var result = StartClient();
            var instance = result.Result.Item2;
            var bus = result.Result.Item1;

            GetsEvent<UserActionSuccessEvent, UserLoginEvent>(instance, new UserLoginEvent() { Name = "a", Password = "a" }, 5).Wait();

            instance.DisconnectClient();
        }

        [Test]
        public void GetsMapResponseForStandingStill()
        {
            var result = StartClient();
            result.Wait();
            var instance = result.Result.Item2;
            GetsEvent<UserActionSuccessEvent, UserLoginEvent>(instance, new UserLoginEvent() { Name = "a", Password = "a" }, 10).Wait();


        }
        static void SendSameGps(ClientInstance instance, CancellationToken ct, GPS gps, int sleeptime)
        {

            do
            {
                instance.SendDebugGPS(gps);
                Thread.Sleep(sleeptime);
                if (ct.IsCancellationRequested)
                    break;

            } while (!ct.IsCancellationRequested);

        }



        [TearDown]
        public void ShutDown()
        {
            server.ShutDownServer().Wait();
        }

    }
}
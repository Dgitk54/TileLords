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
        static void SendDebugGps(ClientInstance instance, CancellationToken ct, int iterations, int sleeptime)
        {
            
            var start = new GPS(49.000000, 7.900000);
            double step = 0.000150;
            var list = DataModelFunctions.GPSNodesWithOffsets(start, step, step, 60);
            
            for(int i = 0; i< iterations; i++)
            {
                instance.SendDebugGPS(list[i]);
                Thread.Sleep(sleeptime);
                if (ct.IsCancellationRequested)
                 break;
            }
            
        }

        [SetUp]
        public void StartServer()
        {
            server = new ServerInstance();
            serverRunning = server.RunServerAsync();
            Thread.Sleep(100);
        }

        [Test]
        public void CanConnect()
        {
            
            var result = StartClient();
            var instance = result.Result.Item2;
            instance.DisconnectClient();
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
            ;
            var instance = result.Result.Item2;
            CancellationTokenSource tokenSrc = new CancellationTokenSource();

            var cleanUp = Task.Run(() => SendDebugGps(instance, tokenSrc.Token,5,1000), tokenSrc.Token);
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
            var cleanUp = Task.Run(() => SendDebugGps(instance, tokenSrc.Token,20,2000), tokenSrc.Token);

           
            
            GetsEvent<MapAsRenderAbleChanged, UserGpsEvent>(instance, new UserGpsEvent(new GPS(49.000000, 7.900000)), 20).Wait();
            Stopwatch watch = Stopwatch.StartNew();
            watch.Start();
            tokenSrc.Cancel();
            cleanUp.Wait();
            instance.DisconnectClient();
            var shutdownDuration = watch.ElapsedMilliseconds;
            ;
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

        [TearDown]
        public void ShutDown()
        {
            server.ShutDownServer().Wait();
        }

    }
}
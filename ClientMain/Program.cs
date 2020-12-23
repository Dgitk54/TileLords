using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using DataModel.Client;
using DataModel.Common;

namespace ClientMain
{
    class Program
    {
        static void Main(string[] args)
        {
           /* var bus = new ClientEventBus();
            var instance = new ClientInstance(bus);
            var task = instance.RunClientAsync(); */





            //LoginTests(instance);
            //SendDebugGps(instance);
            //SendDebugCircle(instance);


            var token = new CancellationTokenSource();


            for (; ; )
            {

                Console.WriteLine("Enter your RegisterUsername:");
                var name = Console.ReadLine();
                if (string.IsNullOrEmpty(name))
                    continue;
                Console.WriteLine("Enter your RegisterPassword");
                var password = Console.ReadLine();
                if (string.IsNullOrEmpty(password))
                    continue;

                Console.WriteLine("Starting with" + name + " password " + password);
                var client1 = Task.Run(() => DebugLoginAndRunAroundClient(name, password, new GPS(49.000000, 7.900000), token.Token));
            }






           // task.Wait();

        }

        static void SendDebugCircle(ClientInstance instance)
        {
            var circleCenter = new GPS(49.000000, 7.900000);
            var nodesAmount = 20;
            var list = DataModelFunctions.GPSNodesInCircle(circleCenter, nodesAmount, 0.001);
            var src = new CancellationTokenSource();
            var runCircle = Task.Run(() => SendGpsPath(instance, src.Token, list, 4000), src.Token);
            Thread.Sleep(120 * 1000);
            src.Cancel();
            runCircle.Wait();

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


        static void SendDebugGps(ClientInstance instance)
        {
            const int periodInSec = 2;
            var obs = Observable.Interval(TimeSpan.FromSeconds(periodInSec),
                                          Scheduler.Default);
            var start = new GPS(49.000000, 7.900000);
            double step = 0.000150;
            var list = DataModelFunctions.GPSNodesWithOffsets(start, 0.000350, 0.000150, 60);
            var counter = 0;
            obs.Subscribe(v =>
            {
                instance.SendDebugGPS(list[counter]);
                counter++;
                if (counter == 10)
                    instance.SendFlawedData();
            });
            Console.ReadLine();
        }
        static void LoginTests(ClientInstance instance)
        {
            Console.WriteLine("1: Create new User,  2: Log in");
            string mode;
            int modeInt;
            do
            {
                mode = Console.ReadLine();
            } while (!Int32.TryParse(mode, out modeInt));



            if (modeInt == 1)
            {
                for (; ; )
                {

                    Console.WriteLine("Enter your RegisterUsername:");
                    var name = Console.ReadLine();
                    if (string.IsNullOrEmpty(name))
                        continue;
                    Console.WriteLine("Enter your RegisterPassword");
                    var password = Console.ReadLine();
                    if (string.IsNullOrEmpty(password))
                        continue;

                    instance.SendRegisterRequest(name, password);

                }
            }
            if (modeInt == 2)
            {
                for (; ; )
                {
                    Console.WriteLine("Enter your LoginUsername:");
                    var name = Console.ReadLine();
                    if (string.IsNullOrEmpty(name))
                        continue;
                    Console.WriteLine("Enter your LoginPassword");
                    var password = Console.ReadLine();
                    if (string.IsNullOrEmpty(password))
                        continue;

                    instance.SendLoginRequest(name, password);

                }


            }
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
            return received.Result;
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

            //var runCircle = Task.Run(() => SendGpsPath(instance, tokenSrc.Token, list, 4000), tokenSrc.Token);

            var sendSame = Task.Run(() => SendSameGps(instance, tokenSrc.Token, circleCenter, 4000));

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
            return (bus, instance, startClient);
        }
    }
}

using DataModel.Client;
using DataModel.Common;
using DataModel.Common.Messages;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ClientMain
{
    class Program
    {

        static void Main(string[] args)
        {

            //LaunchWithChoice(); 
            LaunchSingle10ClientBatch();

        }
        static void LaunchSingle10ClientBatch()
        {
            AutoResetEvent closingEvent = new AutoResetEvent(false);
            for (int i = 0; i < 10; i++)
            {
                Task.Run(() => LargeScaleTestClientDebug());
            }

            closingEvent.WaitOne();
        }
        static void LaunchWithChoice()
        {
            var token = new CancellationTokenSource();
            for (; ; )
            {
                Console.WriteLine("Small or large scale tests? a: small, b: large, c: largescaleobserver");
                var testSize = Console.ReadLine();
                if (string.IsNullOrEmpty(testSize))
                    continue;
                switch (testSize[0])
                {
                    case 'a':
                        SmallScaleTests();
                        break;
                    case 'b':
                        LargeScaleTests();
                        break;
                    case 'c':
                        Task.Run(() => DebugLoginAndObserveTestClient("observer", "observer", GetRandomSpotsInArea(3), token.Token));
                        break;
                    case 'd':
                        LargeScaleTestClientDebug();
                        break;
                }
            }
        }
        static void LargeScaleTestClientDebug()
        {
            var token = new CancellationTokenSource();
            int spots = 3;
            DebugRegisterAndLoginClient("GUID" + Guid.NewGuid().ToString() + DateTime.Now.ToLongTimeString() + DateTime.Now.Millisecond, "test" + DateTime.Now.ToLongTimeString() + DateTime.Now.Millisecond, GetRandomSpotsInArea(spots), token.Token);
        }
        static void LargeScaleTests()
        {

            var token = new CancellationTokenSource();
            int spots = 3;
            for (; ; )
            {
                Console.WriteLine("Enter Clientamount: x 10");

                string input = Console.ReadLine();
                int number;
                if (!Int32.TryParse(input, out number))
                    continue;
                for (int i = 0; i < number; i++)
                {
                    Task.Run(() =>
                    {
                        AutoResetEvent closingEvent = new AutoResetEvent(false);
                        for (int j = 0; j < 10; j++)
                        {
                            Task.Run(() => LargeScaleTestClientDebug());
                        }

                        closingEvent.WaitOne();

                    });
                }
            }




        }
        static void SmallScaleTests()
        {
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

                Console.WriteLine("LocationInt a: GPS(49.000000, 7.900150)        b: GPS(49.000000, 7.900000)          c : GPS(49.000000, 7.900300) ");
                var locationChar = Console.ReadLine();
                if (string.IsNullOrEmpty(password))
                    continue;

                var debugJumpLista = new List<GPS>() { new GPS(49.000000, 7.900300), new GPS(49.000000, 7.900000) };
                var debugJumpListb = new List<GPS>() { new GPS(49.000300, 7.900000), new GPS(49.000600, 7.900000) };
                var debugJumpListc = new List<GPS>() { new GPS(49.000300, 7.900300), new GPS(49.000300, 7.900300) };

                switch (locationChar[0])
                {
                    case 'a':
                        Task.Run(() => DebugLoginAndRunAroundClient(name, password, debugJumpLista, token.Token));
                        break;
                    case 'b':
                        Task.Run(() => DebugLoginAndRunAroundClient(name, password, debugJumpListb, token.Token));
                        break;
                    case 'c':
                        Task.Run(() => DebugLoginAndRunAroundClient(name, password, debugJumpListc, token.Token));
                        break;
                }
            }
        }

        static List<GPS> MainzMiddleSpots => new List<GPS>() { LargeScaleMiddle, new GPS() { Lat = 49.9898735, Lon = 8.2498895 } };
        static List<GPS> MainzSmallScale => new List<GPS>() { new GPS() { Lat = 49.900250, Lon = 8.160250 }, new GPS() { Lat = 49.900300, Lon = 8.160300 } };
        static GPS LargeScaleMiddle => new GPS() { Lat = 49.9898735, Lon = 8.2498895 };

        static void DebugLoginAndObserveTestClient(string name, string password, List<GPS> path, CancellationToken cancellationToken)
        {
            var instance = new ClientInstanceManager("127.0.0.1", 8080, true);
            instance.StartClient();
            ClientFunctions.TryRegisterAndLogInInfiniteAttempts(instance, name, password);
            var tokenSrc = new CancellationTokenSource();

            var sendPath = Task.Run(() => ClientFunctions.SendGpsPath(instance, tokenSrc.Token, path, 4000));
            do
            {
                if (cancellationToken.IsCancellationRequested)
                    break;
                Thread.Sleep(1000);

            } while (!cancellationToken.IsCancellationRequested);

            tokenSrc.Cancel();
            sendPath.Wait();
            instance.ShutDown();

        }
        static void DebugSendSomeMessagesClient(string name, string password, List<GPS> path, CancellationToken cancellationToken)
        {
            var instance = new ClientInstanceManager();
            instance.StartClient();
            instance.SendMessage(new AccountMessage() { Name = name, Password = password, Context = MessageContext.REGISTER });
            Thread.Sleep(1000);
            instance.SendMessage(new AccountMessage() { Name = name, Password = password, Context = MessageContext.LOGIN });
            Thread.Sleep(1000);
            instance.SendMessage(new UserGpsMessage() { Lat = 33.0002, Lon = 12.000035 });
            Thread.Sleep(1000);
            instance.SendMessage(new UserGpsMessage() { Lat = 33.0002, Lon = 12.000035 });
            do
            {
                if (cancellationToken.IsCancellationRequested)
                    break;
                Thread.Sleep(1000);

            } while (!cancellationToken.IsCancellationRequested);
            instance.ShutDown();
        }
        static void DebugRegisterAndLoginClient(string name, string password, List<GPS> path, CancellationToken cancellationToken)
        {
            var instance = new ClientInstanceManager();
            instance.StartClient();
            Thread.Sleep(1000);
            var tokenSrc = new CancellationTokenSource();

            //Try to log in, create account if cant log in:
            ClientFunctions.TryRegisterAndLogInInfiniteAttempts(instance, name, password);


            Console.Write("Connected and logged in!");
            //Thread.Sleep(1000);

            var sendPath = Task.Run(() => ClientFunctions.SendGpsPath(instance, tokenSrc.Token, path, 3000));
            do
            {
                if (cancellationToken.IsCancellationRequested)
                    break;
                Thread.Sleep(1000);

            } while (!cancellationToken.IsCancellationRequested);


            /*do
            {
                if (cancellationToken.IsCancellationRequested)
                    break;
                Thread.Sleep(1000);

            } while (!cancellationToken.IsCancellationRequested); */
            tokenSrc.Cancel();
            sendPath.Wait();
            instance.ShutDown();

        }
        static void DebugLoginAndRunAroundClient(string name, string password, List<GPS> path, CancellationToken cancellationToken)
        {

            var instance = new ClientInstanceManager();
            instance.StartClient();

            var tokenSrc = new CancellationTokenSource();

            //Try to log in, create account if cant log in:
            ClientFunctions.LoginOrRegisterAndLogin(instance, name, password);
            ;
            var sendPath = Task.Run(() => ClientFunctions.SendGpsPath(instance, tokenSrc.Token, path, 4000));
            do
            {
                if (cancellationToken.IsCancellationRequested)
                    break;
                Thread.Sleep(1000);

            } while (!cancellationToken.IsCancellationRequested);

            tokenSrc.Cancel();
            sendPath.Wait();
            instance.ShutDown();
        }

        static List<GPS> GetRandomSpotsInArea(int jumpspots)
        {
            var random = new Random();
            double mainzLatMin = 49.9;  //max should be 50.025
            double mainzLonMin = 8.16;  //max should be 8.334

            var list = new List<GPS>();

            for (int i = 0; i <= jumpspots; i++)
            {
                var rollLat = random.NextDouble() * 0.0005;
                var rollLon = random.NextDouble() * 0.0005;
                list.Add(new GPS() { Lat = mainzLatMin + rollLat, Lon = mainzLonMin + rollLon });
            }

            // list.ConvertAll(v => v.GetPlusCode(10)).ForEach(v => Console.WriteLine(v.Code));
            return list;
        }

    }
}

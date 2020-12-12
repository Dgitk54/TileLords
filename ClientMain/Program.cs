using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using DataModel.Client;
using DataModel.Common;

namespace ClientMain
{
    class Program
    {
        static void Main(string[] args)
        {
            var bus = new ClientEventBus();
            var instance = new ClientInstance(bus);
            var task = instance.RunClientAsync();



            

            LoginTests(instance);
            //SendDebugGps(instance);







            task.Wait();

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
            var mode = Console.ReadLine();
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
    }
}

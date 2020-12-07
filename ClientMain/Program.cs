﻿using System;
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

            const int periodInSec = 2;
            var obs = Observable.Interval(TimeSpan.FromSeconds(periodInSec),
                                          Scheduler.Default);
            var start = new GPS(49.000000, 7.900000);
            double step = 0.000150;
            var list = DataModelFunctions.GPSNodesWithOffsets(start, 0.000350, 0.000150, 60);
            var counter = 0;
            //    obs.Subscribe(v =>
            //    {
            //        instance.SendDebugGPS(list[counter]);
            //        counter++;
            //        if (counter == 10)
            //            instance.SendFlawedData();
            //    });


            var mode = Console.ReadLine();
            int modeInt;
            Console.WriteLine("1: Create new User,  2: Log in");
            do
            {
                mode = Console.ReadLine();
            } while (!Int32.TryParse(mode, out modeInt));



            if (modeInt == 1)
            {
                for (; ; )
                {

                    Console.WriteLine("Enter your Username:");
                    var name = Console.ReadLine();
                    if (string.IsNullOrEmpty(name))
                        continue;
                    Console.WriteLine("Enter your Password");
                    var password = Console.ReadLine();
                    if (string.IsNullOrEmpty(password))
                        continue;

                    instance.SendRegisterRequest(name, password);

                }
            }











            task.Wait();

        }
    }
}

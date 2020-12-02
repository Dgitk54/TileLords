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

            const int periodInSec = 2;
            var obs = Observable.Interval(TimeSpan.FromSeconds(periodInSec),
                                          Scheduler.Default);
            var start = new GPS(49.000000, 7.900000);
            var list = DataModelFunctions.GPSNodesWithOffsets(start, 0.000150, 0.000150, 60);
            var counter = 0;
            obs.Subscribe(v =>
            {
                instance.SendDebugGPS(list[counter]);
                counter++;
            });
            task.Wait();

        }
    }
}

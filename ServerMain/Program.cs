
using DataModel.Server;
using System;

namespace ServerMain
{
    class Program
    {


        public static void Main(String[] args)
        {
            ServerInstance.AttachConsoleLogging();
            var server = new ServerInstance();
            var resourceCleanup = DataBaseFunctions.ResetMapContent();
            Console.WriteLine("Cleaned up resources amount:" + resourceCleanup);
            //var task = server.RunServerAsync();

            server.RunServerAsync().Wait();

            Console.ReadLine();
            //task.Wait();
        }


    }
}


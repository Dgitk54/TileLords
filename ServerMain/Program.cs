
using DataModel.Server;
using StackExchange.Redis;
using System;
using System.Threading;

namespace ServerMain
{
    class Program
    {


        public static void Main(String[] args)
        {
            ServerStart();
        }
        public static void ServerStart()
        {
            //  ServerInstance.AttachConsoleLogging();
            var server = new ServerInstance();
            var resourceCleanup = LiteDBDatabaseFunctions.ResetMapContent();
            Console.WriteLine("Cleaned up resources amount:" + resourceCleanup);

            server.RunServerAsync().Wait();

            Console.ReadLine();
        }
    }
}


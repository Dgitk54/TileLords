
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
            var server = new DotNettyServerInstance();
            server.RunServerAsync().Wait();

            Console.ReadLine();
        }

    }
}


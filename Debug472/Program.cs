using DataModel.Server;
using System;

namespace DebugServer
{
    class Program
    {
        static void Main(string[] args)
        {
            ServerInstance.AttachConsoleLogging();
            var server = new ServerInstance();
            //var task = server.RunServerAsync();

            server.RunServerAsync().Wait();

            Console.ReadLine();
            //task.Wait();
        }
    }
}

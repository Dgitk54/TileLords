
using DataModel.Server;
using System;
using System.Diagnostics;

namespace ServerMain
{
    class Program
    {


        public static void Main(String[] args)
        {
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


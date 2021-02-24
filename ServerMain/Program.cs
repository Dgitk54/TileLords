
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;

using System;
using System.Threading.Tasks;
using DataModel.Server;
using System.Reactive.Linq;
using System.Reactive.Disposables;

namespace ServerMain
{
    class Program
    {


        public static void Main(String[] args)
        {
            var server = new ServerInstance();
            var resourceCleanup = DataBaseFunctions.DeleteAllDatabaseResources();
            Console.WriteLine("Cleaned up resources amount:" + resourceCleanup);
            var task = server.RunServerAsync();



            Console.ReadLine();
            //task.Wait();
        }


    }
}


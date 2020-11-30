
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;

using System;
using System.Threading.Tasks;
using DataModel.Server;

namespace ServerMain
{
    class Program
    {


        public static void Main(String[] args)
        {
            var server = new ServerInstance();
            var task = server.RunServerAsync();
            task.Wait();
        }
    }
}


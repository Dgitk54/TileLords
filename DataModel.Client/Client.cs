using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Common.Internal.Logging;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;

using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DataModel.Client
{

   public class Client
    {
        public static async Task RunClientAsync()
        {
            Console.WriteLine(" IM ALIVE");

            var group = new MultithreadEventLoopGroup();

            var serverIP = IPAddress.Parse("127.0.0.1");
            int serverPort = 8080;

            try
            {
                var bootstrap = new Bootstrap();
                bootstrap
                    .Group(group)
                    .Channel<TcpSocketChannel>()
                    .Option(ChannelOption.TcpNodelay, true) // Do not buffer and send packages right away
                    .Handler(new ActionChannelInitializer<ISocketChannel>(channel =>
                    {
                        IChannelPipeline pipeline = channel.Pipeline;

                        pipeline.AddLast(new ServerHandler());
                    }));

                IChannel bootstrapChannel = await bootstrap.ConnectAsync(new IPEndPoint(serverIP, serverPort));
                
                Console.ReadLine();

                await bootstrapChannel.CloseAsync();
            }
            finally
            {
                group.ShutdownGracefullyAsync().Wait(1000);
            }
        }

        static void Main() => RunClientAsync().Wait();


    }
}




using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using Server;
using System;
using System.Threading.Tasks;

namespace ServerMain
{
    class Program
    {

        private static int port = 8080;

       
        static async Task RunServerAsync()
        {

            var bossGroup = new MultithreadEventLoopGroup(1);
            var workerGroup = new MultithreadEventLoopGroup();
            Console.WriteLine("reached here 1");
            try
            {
                var bootstrap = new ServerBootstrap();
                bootstrap
                    .Group(bossGroup, workerGroup)
                    .Channel<TcpServerSocketChannel>()
                    .Option(ChannelOption.SoBacklog, 100)
                    .ChildHandler(new ActionChannelInitializer<ISocketChannel>(channel =>
                    {
                        IChannelPipeline pipeline = channel.Pipeline;
                        pipeline.AddLast(new TestServerHandler());
                    }));

               
                IChannel bootstrapChannel = await bootstrap.BindAsync(port);

                Console.ReadLine();

                await bootstrapChannel.CloseAsync();
                Console.WriteLine("reached here 2");
            }
            finally
            {
                Task.WaitAll(bossGroup.ShutdownGracefullyAsync(), workerGroup.ShutdownGracefullyAsync());
            }
        }

        public static void Main() => RunServerAsync().Wait();
    }
}


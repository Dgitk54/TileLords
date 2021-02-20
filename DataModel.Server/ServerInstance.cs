using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using System;
using System.Collections.Generic;
using System.Text;
using DotNetty.Handlers.Logging;
using System.Threading.Tasks;
using DotNetty.Codecs;
using DataModel.Common;
using LiteDB;
using System.Security.Cryptography.X509Certificates;
using DotNetty.Handlers.Tls;

namespace DataModel.Server
{
    public class ServerInstance
    {
        
        MultithreadEventLoopGroup bossGroup;//  accepts an incoming connection
        MultithreadEventLoopGroup workerGroup;
        ServerBootstrap bootstrap;
        IChannel bootstrapChannel;
        public ServerInstance()
        {
           
            
        }
        public async Task RunServerAsync()
        {
            var logLevel = LogLevel.INFO;

            var serverPort = 8080;

            if (bootstrap != null)
            {
                throw new Exception("Server already running!");
            }


            bossGroup = new MultithreadEventLoopGroup(1); //  accepts an incoming connection
            workerGroup = new MultithreadEventLoopGroup(10); // handles the traffic of the accepted connection once the boss accepts the connection and registers the accepted connection to the worker

         //   X509Certificate2 tlsCertificate = new X509Certificate2("tilelordss.com.pfx", "mach_irgendwas_random_und_schreibs_auf_XD");

            bootstrap = new ServerBootstrap();

                bootstrap
                    .Group(bossGroup, workerGroup)
                    .Channel<TcpServerSocketChannel>()
                    .Option(ChannelOption.SoBacklog, 100) // maximum queue length for incoming connection
                    .Handler(new LoggingHandler(logLevel))
                    .ChildHandler(new ActionChannelInitializer<ISocketChannel>(channel =>
                    {
                        IChannelPipeline pipeline = channel.Pipeline;
                      //  pipeline.AddLast(TlsHandler.Server(tlsCertificate));
                        pipeline.AddLast("framing-enc", new LengthFieldPrepender(2));
                        pipeline.AddLast("framing-dec", new LengthFieldBasedFrameDecoder(ushort.MaxValue, 0, 2, 0, 2));
                        pipeline.AddLast(new ClientHandler());
                    }));

                bootstrapChannel = await bootstrap.BindAsync(serverPort);

                Console.WriteLine("Server Up");
        }

        public async Task ShutDownServer()
        {
            await bootstrapChannel.CloseAsync();
            Task.WaitAll(bossGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(200), TimeSpan.FromMilliseconds(400)), workerGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(200), TimeSpan.FromMilliseconds(400)));
        }
    }
}

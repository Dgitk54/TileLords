using DotNetty.Codecs;
using DotNetty.Common.Internal.Logging;
using DotNetty.Handlers.Logging;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using MessagePack;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace DataModel.Server
{
    public class ServerInstance
    {
        public static void AttachConsoleLogging()
        {
            var factory = new LoggerFactory();
            factory.AddNLog();
            InternalLoggerFactory.DefaultFactory = factory;
        }
        MultithreadEventLoopGroup bossGroup;//  accepts an incoming connection
        MultithreadEventLoopGroup workerGroup;
        ServerBootstrap bootstrap;
        IChannel bootstrapChannel;
        public ServerInstance()
        {
        }
        public async Task RunServerAsync()
        {
            var serverPort = 8080;

            if (bootstrap != null)
            {
                throw new Exception("Server already running!");
            }
            bossGroup = new MultithreadEventLoopGroup(1); //  accepts an incoming connection
            workerGroup = new MultithreadEventLoopGroup(); // handles the traffic of the accepted connection once the boss accepts the connection and registers the accepted connection to the worker
            var handler = new DebugNonRxClientHandler();
            var options = MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray);
            var encoder = new DotNettyMessagePackEncoder(ref options);
            var decoder = new DotNettyMessagePackDecoder(ref options);
            bootstrap = new ServerBootstrap();
            bootstrap
                .Group(bossGroup, workerGroup)
                .Channel<TcpServerSocketChannel>()
                .Option(ChannelOption.SoBacklog, 100)
                .Option(ChannelOption.SoRcvbuf, int.MaxValue)
                .Handler(new LoggingHandler())
                .ChildHandler(new ActionChannelInitializer<ISocketChannel>(channel =>
                {

                    IChannelPipeline pipeline = channel.Pipeline;
                    pipeline.AddLast(new LoggingHandler("SRV-CONN"));
                    //  pipeline.AddLast(TlsHandler.Server(tlsCertificate));
                    pipeline.AddLast("framing-enc", new LengthFieldPrepender(2));
                    pipeline.AddLast(new DotNettyMessagePackEncoder(ref options));
                    pipeline.AddLast("framing-dec", new LengthFieldBasedFrameDecoder(short.MaxValue, 0, 2, 0, 2));

                    pipeline.AddLast(new DotNettyMessagePackDecoder(ref options));
                    pipeline.AddLast(handler);
                }));


            bootstrapChannel = await bootstrap.BindAsync(serverPort);

            Console.WriteLine("Server Up");
            Console.ReadLine();
        }

        public async Task ShutDownServer()
        {
            await bootstrapChannel.CloseAsync();
            Task.WaitAll(bossGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(200), TimeSpan.FromMilliseconds(400)), workerGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(200), TimeSpan.FromMilliseconds(400)));
        }
    }
}

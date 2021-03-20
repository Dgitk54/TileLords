using DotNetty.Codecs;
using DotNetty.Common;
using DotNetty.Common.Internal.Logging;
using DotNetty.Handlers.Logging;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Libuv;
using MessagePack;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using System;
using System.Net;
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
        DispatcherEventLoopGroup bossGroup;//  accepts an incoming connection
        WorkerEventLoopGroup workerGroup;
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

            ResourceLeakDetector.Level = ResourceLeakDetector.DetectionLevel.Disabled;
            var dispatcher = new DispatcherEventLoopGroup();
            bossGroup = dispatcher;
            workerGroup = new WorkerEventLoopGroup(dispatcher);
            var options = MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray);
            var scheduler = TaskScheduler.Default;
            var handler = new DebugNonRxClientHandler(ref options, scheduler);
            var decoder = new DotNettyMessagePackDecoder(ref options);
            bootstrap = new ServerBootstrap();
            bootstrap
                .Group(bossGroup, workerGroup)
                .Channel<TcpServerChannel>()
                .Option(ChannelOption.SoBacklog, 1024)
               // .Handler(new LoggingHandler())
                .ChildHandler(new ActionChannelInitializer<IChannel>(channel =>
                {

                    IChannelPipeline pipeline = channel.Pipeline;
                    //   pipeline.AddLast(new LoggingHandler("SRV-CONN"));
                    //  pipeline.AddLast(TlsHandler.Server(tlsCertificate));
                    pipeline.AddLast("framing-dec", new LengthFieldBasedFrameDecoder(short.MaxValue, 0, 2, 0, 2));
                  //  pipeline.AddLast(decoder);
                    pipeline.AddLast("framing-enc", new LengthFieldPrepender(2));
                  //  pipeline.AddLast(new DotNettyMessagePackEncoder(ref options));
                    
                    pipeline.AddLast("answerhandler", handler);
                }));


            bootstrapChannel = await bootstrap.BindAsync(IPAddress.Loopback, serverPort);

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

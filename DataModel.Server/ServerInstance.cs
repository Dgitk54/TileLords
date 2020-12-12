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

namespace DataModel.Server
{
    public class ServerInstance
    {
        readonly IEventBus bus = new ServerEventBus();
        readonly ILiteDatabase db = new LiteDatabase(@"MyData.db");
        readonly List<IDisposable> disposables = new List<IDisposable>();
        public ServerInstance()
        {
            ServerFunctions.DebugEventToConsoleSink(bus.GetEventStream<IEvent>());
            disposables.Add(new ClientToClientPositionUpdateHandler(bus).AttachToBus());
            
        }
        public async Task RunServerAsync()
        {
            var logLevel = LogLevel.INFO;


            var serverPort = 8080;

            var bossGroup = new MultithreadEventLoopGroup(1); //  accepts an incoming connection
            var workerGroup = new MultithreadEventLoopGroup(); // handles the traffic of the accepted connection once the boss accepts the connection and registers the accepted connection to the worker


            try
            {
                var bootstrap = new ServerBootstrap();

                bootstrap
                    .Group(bossGroup, workerGroup)
                    .Channel<TcpServerSocketChannel>()
                    .Option(ChannelOption.SoBacklog, 100) // maximum queue length for incoming connection
                    .Handler(new LoggingHandler(logLevel))
                    .ChildHandler(new ActionChannelInitializer<ISocketChannel>(channel =>
                    {
                        IChannelPipeline pipeline = channel.Pipeline;
                        pipeline.AddLast("framing-enc", new LengthFieldPrepender(2));
                        pipeline.AddLast("framing-dec", new LengthFieldBasedFrameDecoder(ushort.MaxValue, 0, 2, 0, 2));
                        pipeline.AddLast(new ClientHandler(bus, db));
                    }));

                IChannel bootstrapChannel = await bootstrap.BindAsync(serverPort);

                Console.WriteLine("Server Up");
                Console.ReadLine();

                await bootstrapChannel.CloseAsync();
            }
            finally
            {
                Task.WaitAll(bossGroup.ShutdownGracefullyAsync(), workerGroup.ShutdownGracefullyAsync());
            }
        }
    }
}

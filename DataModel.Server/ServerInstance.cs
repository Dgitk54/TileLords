﻿using DotNetty.Codecs;
using DotNetty.Common.Internal.Logging;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using NLog.Extensions.Logging;
using DotNetty.Handlers.Logging;
using DotNetty.Handlers.Flush;

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
            DataBaseFunctions.WipeAllDatabases();
            DataBaseFunctions.InitializeDataBases();
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
            bootstrap = new ServerBootstrap();
            bootstrap
                .Group(bossGroup, workerGroup)
                .Channel<TcpServerSocketChannel>()
                .Option(ChannelOption.SoBacklog, 1024)
                .Handler(new LoggingHandler())
                //.ChildOption(ChannelOption.SoReuseaddr, true)
                //.ChildOption(ChannelOption.SoReuseport, false)
                //.ChildOption(ChannelOption.SoBroadcast, true)
                //.ChildOption(ChannelOption.SoKeepalive, true)
                .ChildOption(ChannelOption.TcpNodelay, true)
                .ChildOption(ChannelOption.SoSndbuf, 2048)
                .ChildOption(ChannelOption.SoRcvbuf, 8196)
                .ChildHandler(new ActionChannelInitializer<ISocketChannel>(channel =>
                {
                    IChannelPipeline pipeline = channel.Pipeline;
                    pipeline.AddLast(new FlushConsolidationHandler());
                    pipeline.AddLast(new LoggingHandler("SRV-CONN"));
                    pipeline.AddLast("framing-enc", new LengthFieldPrepender(2));
                    pipeline.AddLast("framing-dec", new LengthFieldBasedFrameDecoder(short.MaxValue, 0, 2, 0, 2));
                    pipeline.AddLast(new DotNettyMessagePackDecoder());
                    pipeline.AddLast(new DotNettyMessagePackEncoder());
                    pipeline.AddLast(new ClientHandler());
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

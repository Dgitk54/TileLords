using DataModel.Common;
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Common.Internal.Logging;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;

using System;
using System.Net;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace DataModel.Client
{

    public class ClientInstance
    {
        public Subject<string> ReceivedData { get;}
        ServerHandler ServerHandler {get;}
        Bootstrap Bootstrap { get; set; }
        IChannel BootstrapChannel { get; set; }

        readonly MultithreadEventLoopGroup group = new MultithreadEventLoopGroup();
        public ClientInstance()
        {
            ServerHandler = new ServerHandler(this);
            ReceivedData = new Subject<string>();
        }



        public void SendGPS(GPS gps)
        {
            ServerHandler.GPSSource.OnNext(gps);
        }
        
        public async Task DisconnectClient()
        {
            await BootstrapChannel.CloseAsync();
            ServerHandler.GPSSource.OnCompleted();
            group.ShutdownGracefullyAsync().Wait();
        }


        public async Task RunClientAsync()
        {
            
            var serverIP = IPAddress.Parse("127.0.0.1");
            int serverPort = 8080;

            
                Bootstrap = new Bootstrap();
                Bootstrap
                    .Group(group)
                    .Channel<TcpSocketChannel>()
                    .Option(ChannelOption.TcpNodelay, true) // Do not buffer and send packages right away
                    .Handler(new ActionChannelInitializer<ISocketChannel>(channel =>
                    {
                        IChannelPipeline pipeline = channel.Pipeline;
                        pipeline.AddLast("framing-enc", new LengthFieldPrepender(2));
                        pipeline.AddLast("framing-dec", new LengthFieldBasedFrameDecoder(ushort.MaxValue, 0, 2, 0, 2));
                        pipeline.AddLast(ServerHandler);
                    }));

                IChannel bootstrapChannel = await Bootstrap.ConnectAsync(new IPEndPoint(serverIP, serverPort));


                
            
        }



    }
}



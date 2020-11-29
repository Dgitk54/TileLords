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

        private ClientHandler ClientHandler {get;}

        public ClientInstance()
        {
            ClientHandler = new ClientHandler();
        }

        public void SendGPS(GPS gps)
        {
            ClientHandler.GPSSource.OnNext(gps);
        }
        
        public async Task RunClientAsync()
        {
            Console.WriteLine("ClientAlive");

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
                        pipeline.AddLast("framing-enc", new LengthFieldPrepender(2));
                        pipeline.AddLast("framing-dec", new LengthFieldBasedFrameDecoder(ushort.MaxValue, 0, 2, 0, 2));
                        pipeline.AddLast(ClientHandler);
                    }));

                IChannel bootstrapChannel = await bootstrap.ConnectAsync(new IPEndPoint(serverIP, serverPort));

                Console.ReadLine();

                await bootstrapChannel.CloseAsync();
            }
            finally
            {
                Console.WriteLine("CLOSING!");
                ClientHandler.GPSSource.OnCompleted();
                group.ShutdownGracefullyAsync().Wait();
            }
        }



    }
}



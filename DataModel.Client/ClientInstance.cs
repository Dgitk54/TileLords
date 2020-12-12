using DataModel.Common;
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Common.Internal.Logging;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DataModel.Client
{

    public class ClientInstance
    {

        public IEventBus EventBus { get => eventBus; }
        

        ServerHandler ServerHandler { get; }
        Bootstrap Bootstrap { get; set; }
        IChannel BootstrapChannel { get; set; }


        readonly MultithreadEventLoopGroup group = new MultithreadEventLoopGroup();
        readonly List<IDisposable> disposables = new List<IDisposable>();
        readonly JsonSerializerSettings settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All
        };

        readonly CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
        private readonly IEventBus eventBus;
        public ClientInstance(IEventBus bus)
        {
            ServerHandler = new ServerHandler(bus);
            eventBus = bus;
            disposables.Add(ClientFunctions.DebugEventToConsoleSink(eventBus.GetEventStream<IEvent>()));
        }


        public void SendDebugGPS(GPS gps) => eventBus.Publish(new UserGpsEvent(gps));

        public void SendFlawedData() => eventBus.Publish(new DataSinkEvent("TEST123ää²³"));

        public void SendRegisterRequest(string username, string password)
        {
            var e = new UserRegisterEvent() { Name = username, Password = password };
            var debugRegisterEvent = new DataSinkEvent(JsonConvert.SerializeObject(e, typeof(UserRegisterEvent), settings));

            eventBus.Publish(debugRegisterEvent);
        }
        public void SendLoginRequest(string username, string password)
        {
            
            var e = new UserLoginEvent() { Name = username, Password = password };
            var debugRegisterEvent = new DataSinkEvent(JsonConvert.SerializeObject(e, typeof(UserLoginEvent), settings));

            eventBus.Publish(debugRegisterEvent);
        }

        public void DisconnectClient()
        {
            cancelTokenSource.Cancel();
            ServerHandler.ShutDown();
        }


        public async Task RunClientAsync()
        {
            try
            {
                do
                {
                    if (Bootstrap != null)
                    {
                        throw new Exception("Client already running!");
                    }
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

                    BootstrapChannel = await Bootstrap.ConnectAsync(new IPEndPoint(serverIP, serverPort));



                } while (!cancelTokenSource.IsCancellationRequested);
                

            }
            finally
            {
                Task.WaitAll(group.ShutdownGracefullyAsync());
            }
                
            
            
        }



    }
}



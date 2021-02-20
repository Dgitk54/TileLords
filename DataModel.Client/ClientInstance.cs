using DataModel.Common;
using DataModel.Common.Messages;
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Common.Internal.Logging;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DataModel.Client
{

    public class ClientInstance
    {
        Bootstrap Bootstrap { get; set; }
        IChannel BootstrapChannel { get; set; }

        readonly ServerHandler serverHandler;

        readonly MultithreadEventLoopGroup group = new MultithreadEventLoopGroup();
        readonly List<IDisposable> disposables = new List<IDisposable>();

        readonly Subject<UnityMapMessage> mapSubject = new Subject<UnityMapMessage>();
        readonly Subject<IMsgPackMsg> outboundTraffic = new Subject<IMsgPackMsg>();

        static readonly AutoResetEvent closingEvent = new AutoResetEvent(false);

        public ClientInstance()
        {
            serverHandler = new ServerHandler(this);
            var localMapUpdate = GetBigMapStream(outboundTraffic.OfType<UserGpsMessage>());

            disposables.Add(GetSmallUnityMap(outboundTraffic.OfType<UserGpsMessage>(), localMapUpdate).Subscribe(v => mapSubject.OnNext(v)));
        }

        public IObservable<UnityMapMessage> ClientMapStream => mapSubject.AsObservable();

        public IObservable<IMsgPackMsg> OutboundTraffic => outboundTraffic.AsObservable();

        public IObservable<IMsgPackMsg> InboundTraffic => serverHandler.InboundTraffic;

        public IObservable<bool> ClientConnectionState => serverHandler.ConnctionState;

        public void SendGps(GPS gps) => SendMessage(new UserGpsMessage() {Lat = gps.Lat, Lon = gps.Lon });


        public void SendRegisterRequest(string username, string password)
        {
            var e = new AccountMessage() { Name = username, Password = password, Context = MessageContext.REGISTER};
            SendMessage(e);
        }

        public void SendLoginRequest(string username, string password)
        {

            var e = new AccountMessage() { Name = username, Password = password, Context = MessageContext.LOGIN};
            SendMessage(e);
        }

        public void SendMessage(IMsgPackMsg msg)
        {
            outboundTraffic.OnNext(msg);
        }

        public void DisconnectClient()
        {
            serverHandler.ShutDown();
            closingEvent.Set();
            disposables.ForEach(v => v.Dispose());
            if(BootstrapChannel != null)
                BootstrapChannel.CloseAsync().Wait();
            Task.WaitAll(group.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(200), TimeSpan.FromMilliseconds(400)));
        }
        public async Task RunClientAsyncWithIP(string ipAdress = "127.0.0.1", int port = 8080)
        {
            try
            {

                if (Bootstrap != null)
                {
                    throw new Exception("Client already running!");
                }
                var serverIP = IPAddress.Parse(ipAdress);
                int serverPort = port;

            //    X509Certificate2 cert = null;
            //    string targetHost = null;
            //    cert = new X509Certificate2("name", "password");
            //    targetHost = cert.GetNameInfo(X509NameType.DnsName, false);

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
                        pipeline.AddLast(serverHandler);
                    }));

                BootstrapChannel = await Bootstrap.ConnectAsync(new IPEndPoint(serverIP, serverPort));
                closingEvent.WaitOne();
            }
            finally
            {

                //Task.WaitAll(group.ShutdownGracefullyAsync());
            }
        }

        IObservable<UnityMapMessage> GetSmallUnityMap(IObservable<UserGpsMessage> gpsStream, IObservable<ServerMapEvent> bigMapStream)
        {
            return gpsStream.Select(v => new GPS(v.Lat, v.Lon))
                            .Select(v => v.GetPlusCode(10))
                            .DistinctUntilChanged()
                            .Select(v => (v, LocationCodeTileUtility.GetTileSection(v.Code, 10, 10)))
                            .WithLatestFrom(bigMapStream.Select(v => v.Tiles), (loc, map) => new { loc, map })
                            .Select(v =>
                            {
                                return (v.loc.v, v.map.ToList().SelectMany(v2 => v2.MiniTiles).Where(v2 => v.loc.Item2.Contains(v2.MiniTileId.Code)).ToList());
                            })
                            .Select(v => (v.v, LocationCodeTileUtility.SortList(v.Item2)))
                            .Select(v => new UnityMapMessage() { ClientLocation = v.v, VisibleMap = v.Item2 });
        }


        IObservable<ServerMapEvent> GetBigMapStream(IObservable<UserGpsMessage> messageStream)
        {
            return messageStream.Select(v => new GPS(v.Lat, v.Lon))
                                .Select(v => v.GetPlusCode(8))
                                .DistinctUntilChanged()
                                .Select(v => LocationCodeTileUtility.GetTileSection(v.Code, 1, v.Precision)) //List of local area strings
                                .Select(v => v.ConvertAll(e => new PlusCode(e, 8))) //transform into pluscodes
                                .Select(v => v.ConvertAll(e => WorldGenerator.GenerateTile(e))) //transform each pluscode into world tile
                                .Select(v => new ServerMapEvent() { Tiles = v });
        }
    }
}



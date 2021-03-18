using DataModel.Common;
using DataModel.Common.Messages;
using DotNetty.Codecs;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reactive.Linq;
using System.Reactive.Subjects;
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
        readonly Subject<IMessage> outboundTraffic = new Subject<IMessage>();

        readonly ActionChannelInitializer<ISocketChannel> actionChannelInitializer;

        static readonly AutoResetEvent closingEvent = new AutoResetEvent(false);

        public ClientInstance()
        {
            
            serverHandler = new ServerHandler(this);
            actionChannelInitializer = new ActionChannelInitializer<ISocketChannel>(channel =>
            {
                IChannelPipeline pipeline = channel.Pipeline;

                pipeline.AddLast("framing-enc", new LengthFieldPrepender(2));
                pipeline.AddLast("framing-dec", new LengthFieldBasedFrameDecoder(ushort.MaxValue, 0, 2, 0, 2));
               // pipeline.AddLast(new DelimiterBasedFrameDecoder(1024,true,true,Delimiters.LineDelimiter()));
                pipeline.AddLast(new DotNettyMessagePackEncoder());
                pipeline.AddLast(new DotNettyMessagePackDecoder());
                pipeline.AddLast(serverHandler);
            });
        }
        public IDisposable AddOutBoundTraffic(IObservable<IMessage> outboundSource)
        {
            return outboundSource.Subscribe(v => outboundTraffic.OnNext(v));
        }
        public IObservable<UnityMapMessage> ClientMapStream => mapSubject.AsObservable();

        public IObservable<IMessage> OutboundTraffic => outboundTraffic.AsObservable();

        public IObservable<IMessage> InboundTraffic => serverHandler.InboundTraffic;

        public IObservable<bool> ClientConnectionState => serverHandler.ConnctionState;

        public void SendGps(GPS gps)
        {
            SendMessage(new UserGpsMessage() { Lat = gps.Lat, Lon = gps.Lon });
        }

        public void SendRegisterRequest(string username, string password)
        {
            var e = new AccountMessage() { Name = username, Password = password, Context = MessageContext.REGISTER };
            SendMessage(e);
        }

        public void SendLoginRequest(string username, string password)
        {

            var e = new AccountMessage() { Name = username, Password = password, Context = MessageContext.LOGIN };
            SendMessage(e);
        }

        public void SendMessage(IMessage msg)
        {
            outboundTraffic.OnNext(msg);
        }

        public void DisconnectClient()
        {
            serverHandler.ShutDown();
            closingEvent.Set();
            disposables.ForEach(v => v.Dispose());
            if (BootstrapChannel != null)
                BootstrapChannel.DisconnectAsync().Wait();
            closingEvent.Reset();
        }
        public void CloseClient()
        {
            serverHandler.ShutDown();
            closingEvent.Set();
            disposables.ForEach(v => v.Dispose());
            if (BootstrapChannel != null)
                BootstrapChannel.CloseAsync().Wait();
            Task.WaitAll(group.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(200), TimeSpan.FromMilliseconds(400)));
            closingEvent.Reset();
        }
        public async Task RunClientAsyncWithIP(string ipAdress = "127.0.0.1", int port = 8080)
        {
            var localMapUpdate = GetBigMapStream(outboundTraffic.OfType<UserGpsMessage>());
            disposables.Add(GetSmallUnityMap(outboundTraffic.OfType<UserGpsMessage>(), localMapUpdate).Subscribe(v => mapSubject.OnNext(v)));
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
                    .Handler(actionChannelInitializer);

                BootstrapChannel = await Bootstrap.ConnectAsync(new IPEndPoint(serverIP, serverPort));
                closingEvent.WaitOne();
            }
            finally
            {

                //Task.WaitAll(group.ShutdownGracefullyAsync());
            }
        }

        IObservable<UnityMapMessage> GetSmallUnityMap(IObservable<UserGpsMessage> gpsStream, IObservable<Dictionary<PlusCode, MiniTile>> bigMapStream)
        {
            return gpsStream.Select(v => new GPS(v.Lat, v.Lon))
                            .Select(v => v.GetPlusCode(10))
                            .DistinctUntilChanged()
                            .Select(v => (v, LocationCodeTileUtility.GetTileSection(v.Code, 10, 10)))
                            .WithLatestFrom(bigMapStream, (loc, map) => new { loc, map })
                            .Select(v =>
                            {
                                var map = v.loc.Item2.ConvertAll(e => e.From10String());
                                var transformedMap = map.Select(e =>
                                 {
                                     MiniTile tile = null;
                                     v.map.TryGetValue(e, out tile);
                                     return tile;
                                 }).ToList();

                                return (v.loc.v, transformedMap);
                            })
                            .Select(v => new UnityMapMessage() { ClientLocation = v.v, VisibleMap = v.transformedMap });
        }


        IObservable<Dictionary<PlusCode, MiniTile>> GetBigMapStream(IObservable<UserGpsMessage> messageStream)
        {
            return messageStream.Select(v => new GPS(v.Lat, v.Lon))
                                 .Select(v => v.GetPlusCode(8))
                                 .DistinctUntilChanged()
                                 .Select(v => LocationCodeTileUtility.GetTileSection(v.Code, 1, v.Precision)) //List of local area strings
                                 .Select(v => v.ConvertAll(e => new PlusCode(e, 8))) //transform into pluscodes
                                 .Select(v => v.ConvertAll(e => WorldGenerator.GenerateTile(e, "extensionToSeed"))) //transform each pluscode into world tile
                                 .Select(v => v.ConvertAll(e => e.MiniTiles))
                                 .Select(v => v.SelectMany(e => e).ToList())
                                 .Select(v => v.GroupBy(e => e.MiniTileId).ToDictionary(e => e.Key, e => e.First()));
        }
    }
}



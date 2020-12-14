using DataModel.Common;
using DotNetty.Buffers;
using DotNetty.Common.Internal.Logging;
using DotNetty.Transport.Channels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Joins;
using System.Reactive.Subjects;
using System.Diagnostics;
using System.Reactive.Concurrency;
using LiteDB;

namespace DataModel.Server
{
    public class ClientHandler : ChannelHandlerAdapter
    {
        static readonly IInternalLogger Logger = InternalLoggerFactory.GetInstance<ClientHandler>();

        readonly Subject<string> jsonClientSource = new Subject<string>();

        readonly List<IDisposable> disposables = new List<IDisposable>();

        readonly IEventBus serverBus; // eventbus for serverwide messages
        readonly IEventBus clientBus; // eventbus for clientwide messages

        readonly ClientChunkUpdateHandler gpsClientLocationHandler;
        readonly ClientAccountRegisterHandler registerHandler;


        public ClientHandler(IEventBus serverBus)
        {
            this.serverBus = serverBus;
            clientBus = new ClientEventBus();


            gpsClientLocationHandler = new ClientChunkUpdateHandler(clientBus);
            registerHandler = new ClientAccountRegisterHandler(clientBus,serverBus);
            
        }

        public override void ChannelActive(IChannelHandlerContext ctx)
        {
            var loginHandler = new ClientAccountLoginHandler(clientBus, serverBus, ctx.Channel);
            serverBus.Publish(new ServerClientConnectedEvent() { Channel = ctx.Channel });
            disposables.Add(ServerFunctions.EventStreamSink(clientBus.GetEventStream<DataSinkEvent>(), ctx));
            disposables.Add(jsonClientSource.Subscribe(v => clientBus.Publish(new DataSourceEvent(v))));
            disposables.Add(gpsClientLocationHandler.AttachToBus());
            disposables.Add(registerHandler.AttachToBus());
            disposables.Add(loginHandler.AttachToBus());
            ctx.Channel.CloseCompletion.ContinueWith((x) => Console.WriteLine("Channel Closed"));
        }

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            var byteBuffer = message as IByteBuffer;
            if (byteBuffer != null)
            {
                Console.WriteLine("Received from client: " + byteBuffer.ToString(Encoding.UTF8));
                jsonClientSource.OnNext(byteBuffer.ToString(Encoding.UTF8));
            }
        }

        // The Channel is closed hence the connection is closed
        public override void ChannelInactive(IChannelHandlerContext ctx)
        {
            serverBus.Publish(new ServerClientDisconnectedEvent() { Channel = ctx.Channel});

            disposables.ForEach(v => v.Dispose());
            Console.WriteLine("Cleaned up ClientHandler");
        }

        


        public override bool IsSharable => true;
    }


}

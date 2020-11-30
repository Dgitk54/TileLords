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

namespace DataModel.Server
{
    public class ClientHandler : ChannelHandlerAdapter
    {
        static readonly IInternalLogger Logger = InternalLoggerFactory.GetInstance<ClientHandler>();

        private readonly Subject<string> jsonClientSource = new Subject<string>();
        private readonly ServerFunctions functions = new ServerFunctions();

        private List<IDisposable> disposables = new List<IDisposable>();

        private readonly IEventBus serverBus;
        private readonly IEventBus clientBus;

        private readonly ClientLocationHandler gpsClientLocationHandler;
        public ClientHandler(IEventBus serverBus)
        {
            this.serverBus = serverBus;
            clientBus = new ClientEventBus();
            gpsClientLocationHandler = new ClientLocationHandler(clientBus);
        }
        public override void ChannelActive(IChannelHandlerContext ctx)
        {
            serverBus.Publish<ClientConnectedEvent>(new ClientConnectedEvent(ctx.Name));
            disposables.Add(functions.EventStreamSink(clientBus.GetEventStream<DataSinkEvent>(), ctx));
            disposables.Add(jsonClientSource.Subscribe(v => clientBus.Publish<DataSourceEvent>(new DataSourceEvent(v))));
            disposables.Add(gpsClientLocationHandler.AttachToBus());
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
            serverBus.Publish<ClientDisconnectedEvent>(new ClientDisconnectedEvent(ctx.Name));
            disposables.ForEach(v => v.Dispose());
        }



        public override bool IsSharable => true;
    }


}

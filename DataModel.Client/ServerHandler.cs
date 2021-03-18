using DataModel.Common;
using DataModel.Common.Messages;
using DotNetty.Buffers;
using DotNetty.Common.Internal.Logging;
using DotNetty.Transport.Channels;
using System;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;

namespace DataModel.Client
{
    public class ServerHandler : ChannelHandlerAdapter
    {

        static readonly IInternalLogger Logger = InternalLoggerFactory.GetInstance<ServerHandler>();

        readonly Subject<IMessage> inboundTraffic = new Subject<IMessage>();
        readonly BehaviorSubject<bool> connectionState = new BehaviorSubject<bool>(false);
        readonly ClientInstance instance;
        IDisposable outBoundManager;


        public ServerHandler(ClientInstance instance)
        {
            this.instance = instance;

        }
        public IObservable<IMessage> InboundTraffic => inboundTraffic.AsObservable();

        public IObservable<bool> ConnctionState => connectionState.AsObservable();

        public override void ChannelActive(IChannelHandlerContext context)
        {
            outBoundManager = instance.OutboundTraffic.Subscribe(v => TaskPoolScheduler.Default.Schedule(() => context.WriteAndFlushAsync(v)));
            connectionState.OnNext(true);
        }

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            
            var asMsg = message as IMessage;
           // Console.WriteLine(asMsg.ToString());
            TaskPoolScheduler.Default.Schedule(() => inboundTraffic.OnNext(asMsg) );
               
        }

        public override void ChannelInactive(IChannelHandlerContext ctx)
        {
            connectionState.OnNext(false);
            outBoundManager.Dispose();
        }

        public void ShutDown()
        {
            if (outBoundManager != null)
            {
                outBoundManager.Dispose();
            }
        }

        public override bool IsSharable => true;
    }


}
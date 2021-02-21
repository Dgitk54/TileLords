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
using Newtonsoft.Json;
using System.Reactive.Concurrency;
using DataModel.Common.Messages;

namespace DataModel.Client
{
    public class ServerHandler : ChannelHandlerAdapter
    {

        static readonly IInternalLogger Logger = InternalLoggerFactory.GetInstance<ServerHandler>();

        readonly Subject<IMsgPackMsg> inboundTraffic = new Subject<IMsgPackMsg>();
        readonly BehaviorSubject<bool> connectionState = new BehaviorSubject<bool>(false);
        readonly ClientInstance instance;
        IDisposable outBoundManager;


        public ServerHandler(ClientInstance instance)
        {
            this.instance = instance;

        }
        public IObservable<IMsgPackMsg> InboundTraffic => inboundTraffic.AsObservable();

        public IObservable<bool> ConnctionState => connectionState.AsObservable();

        public override void ChannelActive(IChannelHandlerContext context)
        {
            outBoundManager = instance.OutboundTraffic.Select(v => v.ToJsonPayload()).Subscribe(v =>
             {
                 Console.WriteLine("PUSHING: DATA" + v.GetLength(0));
                 context.WriteAndFlushAsync(Unpooled.WrappedBuffer(v));
             });
            connectionState.OnNext(true);
        }

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            var byteBuffer = message as IByteBuffer;
            if (byteBuffer != null)
            {
                Console.WriteLine("Received" + byteBuffer.ToString(Encoding.UTF8));
                var msgpack = byteBuffer.ToString(Encoding.UTF8).FromString();
                inboundTraffic.OnNext(msgpack);
            }
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
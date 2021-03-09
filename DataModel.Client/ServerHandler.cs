﻿using DataModel.Common;
using DataModel.Common.Messages;
using DotNetty.Buffers;
using DotNetty.Common.Internal.Logging;
using DotNetty.Transport.Channels;
using System;
using System.Linq;
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
            outBoundManager = instance.OutboundTraffic.Select(v => v.ToJsonPayload()).Subscribe(v =>
             {
                 context.WriteAndFlushAsync(Unpooled.WrappedBuffer(v));
             });
            connectionState.OnNext(true);
        }

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            var byteBuffer = message as IByteBuffer;
            if (byteBuffer != null)
            {
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
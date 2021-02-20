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
using MessagePack;
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
            outBoundManager = instance.OutboundTraffic.Subscribe(v =>
            {
                var lz4Options = MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray);
                var data = MessagePackSerializer.Serialize(v, lz4Options);
                Console.WriteLine("PUSHING: DATA" + data.GetLength(0));
                context.WriteAndFlushAsync(Unpooled.WrappedBuffer(data));
            });
            connectionState.OnNext(true);

        }

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            var byteBuffer = message as IByteBuffer;
            if (byteBuffer != null)
            {
                var lz4Options = MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray);
                IMsgPackMsg data = null;
                try
                {
                    data = MessagePackSerializer.Deserialize<IMsgPackMsg>(byteBuffer.Array, lz4Options);

                }
                catch (MessagePackSerializationException e)
                {
                    Console.WriteLine("Error Deserializing" + e.ToString());
                }
                if (data != null)
                {
                    inboundTraffic.OnNext(data);
                }
            }
        }

        public override void ChannelInactive(IChannelHandlerContext ctx)
        {
            connectionState.OnNext(false);
            outBoundManager.Dispose();
        }

        public void ShutDown() => outBoundManager.Dispose();

        public override bool IsSharable => true;
    }


}
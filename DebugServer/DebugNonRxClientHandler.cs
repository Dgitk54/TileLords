using DataModel.Common.Messages;
using DataModel.Common;
using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using MessagePack;
using System;
using System.Threading.Tasks;

namespace DataModel.Server
{
    public class DebugNonRxClientHandler : ChannelHandlerAdapter
    {
        readonly MessagePackSerializerOptions lz4Options;
        readonly TaskScheduler scheduler;
        readonly TaskFactory factory;
        public DebugNonRxClientHandler(ref MessagePackSerializerOptions options, TaskScheduler scheduler)
        {
            lz4Options = options;
            this.scheduler = scheduler;
            factory = new TaskFactory(scheduler);
        }

        public override void ChannelRead(IChannelHandlerContext context, object msg)
        {
            
            factory.StartNew(() =>
            {
                var castedMessage = (IByteBuffer)msg;

                int length = castedMessage.ReadableBytes;
                var array = new byte[length];
                castedMessage.GetBytes(castedMessage.ReaderIndex, array, 0, length);
                var deserialized = MessagePackSerializer.Deserialize<IMessage>(array, lz4Options);
                switch (deserialized)
                {
                    case AccountMessage x:
                        if (x.Context == MessageContext.REGISTER)
                        {
                            HandleAccountMessage(context, x); 
                        }
                        if (x.Context == MessageContext.LOGIN)
                        {
                            HandleRegisterMessage(context, x);    
                        }
                        break;
                }
            });
        }
        
        async void HandleAccountMessage(IChannelHandlerContext context, AccountMessage msg)
        {
            IMessage tmp = new UserActionMessage() { MessageContext = MessageContext.REGISTER, MessageState = MessageState.SUCCESS };
            var serialized = MessagePackSerializer.Serialize(tmp, lz4Options);
            var buffer = Unpooled.WrappedBuffer(serialized);
            await context.WriteAndFlushAsync(buffer);
        }
        async void HandleRegisterMessage(IChannelHandlerContext context, AccountMessage msg)
        {
            IMessage tmp = new UserActionMessage() { MessageContext = MessageContext.LOGIN, MessageState = MessageState.SUCCESS };
            var serialized = MessagePackSerializer.Serialize(tmp, lz4Options);
            var buffer = Unpooled.WrappedBuffer(serialized);
            await context.WriteAndFlushAsync(buffer);
        }

        public override bool IsSharable => true;


    }
}

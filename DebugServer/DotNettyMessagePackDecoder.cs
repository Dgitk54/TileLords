using DataModel.Common.Messages;
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using MessagePack;
using System.Collections.Generic;

namespace DataModel.Server
{
    public class DotNettyMessagePackDecoder : MessageToMessageDecoder<IByteBuffer> 
    {
        readonly MessagePackSerializerOptions lz4Options;
        public DotNettyMessagePackDecoder(ref MessagePackSerializerOptions options)
        {
            lz4Options = options;
        }
      
        protected override void Decode(IChannelHandlerContext context, IByteBuffer message, List<object> output)
        {
            int length = message.ReadableBytes;
            var array = new byte[length];
            message.GetBytes(message.ReaderIndex, array, 0, length);
            var deserialized = MessagePackSerializer.Deserialize<IMessage>(array, lz4Options);
            output.Add(deserialized);
        }

        public override bool IsSharable => true;
    }
}

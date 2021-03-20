using DataModel.Common.Messages;
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using MessagePack;
using System.Collections.Generic;

namespace DataModel.Server
{
    public class DotNettyMessagePackDecoder : ByteBufferToMessageDecoder
    {
        readonly MessagePackSerializerOptions lz4Options;
        public DotNettyMessagePackDecoder(ref MessagePackSerializerOptions options)
        {
            lz4Options = options;
        }
        protected override void Decode(IChannelHandlerContext context, ref ByteBufferReader reader, List<object> output)
        {
            var array = reader.UnreadSpan.ToArray();
            var deserialized = MessagePackSerializer.Deserialize<IMessage>(array, lz4Options);
            if (deserialized != null)
            {
                output.Add(deserialized);
            }
        }
    }
}

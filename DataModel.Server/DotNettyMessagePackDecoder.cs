using DataModel.Common.Messages;
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataModel.Server
{
    public class DotNettyMessagePackDecoder : ByteToMessageDecoder
    {
        MessagePackSerializerOptions lz4Options = MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray);
        protected override void Decode(IChannelHandlerContext context, IByteBuffer message, List<object> output)
        {
            if (message.IsReadable())
            {
                var length = message.ReadableBytes;
                var array = new byte[length];
                message.GetBytes(message.ReaderIndex, array, 0, length);
                var deserialized = MessagePackSerializer.Deserialize<IMessage>(array, lz4Options);
                if (deserialized != null)
                {
                    output.Add(deserialized);
                    message.AdvanceReader(length);
                }
                else
                {
                    message.Clear();
                }

            }

        }

    }
}

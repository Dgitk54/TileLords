using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataModel.Client
{
    public class DotNettyMessagePackDecoder : MessageToMessageDecoder<IByteBuffer>
    {
        protected override void Decode(IChannelHandlerContext context, IByteBuffer message, List<object> output)
        {
            try
            {
                var length = message.ReadableBytes;
                var array = new byte[length];
                message.GetBytes(message.ReaderIndex, array, 0, length);
                var deserialized = MessagePackSerializer.Deserialize<object>(array, MessagePack.Resolvers.TypelessContractlessStandardResolver.Options);
                output.Add(deserialized);
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR DECODING!");
            }
            
        }
    }
}

using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DataModel.Server
{
    public class DotNettyMessagePackEncoder : MessageToByteEncoder<object>
    {
        protected override void Encode(IChannelHandlerContext context, object message, IByteBuffer output)
        {
            var data = MessagePackSerializer.Serialize(message, MessagePack.Resolvers.TypelessContractlessStandardResolver.Options);
            output.WriteBytes(data);
        }
    }
}

using DataModel.Common.Messages;
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using MessagePack;
using System.Threading.Tasks;

namespace DataModel.Server
{
    public class DotNettyMessagePackEncoder : MessageToByteEncoder<IMessage>
    {
        MessagePackSerializerOptions lz4Options;
        public DotNettyMessagePackEncoder(ref MessagePackSerializerOptions options)
        {
            lz4Options = options;
        }
        protected override void Encode(IChannelHandlerContext context, IMessage message, IByteBuffer output)
        {
            
                var data = MessagePackSerializer.Serialize(message, lz4Options);
                output.WriteBytes(data);
            

        }

    }
}

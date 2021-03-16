using DataModel.Common.Messages;
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
        MessagePackSerializerOptions lz4Options = MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray);
        protected override void Encode(IChannelHandlerContext context, object message, IByteBuffer output)
        {
            if(message is IMessage)
            {
                var msg = message as IMessage;
                var data = MessagePackSerializer.Serialize(msg, lz4Options);
                output.WriteBytes(data);
            } 
            else
            {
                throw new Exception("Encoding non messagepack objects");
            }
           
        }
        public override bool IsSharable => true;
    }
}

using DataModel.Common.Messages;
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataModel.Server
{
    public class DotNettyByteToMessageDecoder : ByteToMessageDecoder
    {
       
        protected override void Decode(IChannelHandlerContext context, IByteBuffer input, List<object> output)
        {
            try
            {
                ;
                var obj = MessagePackSerializer.Deserialize<IMsgPackMsg>(input.ReadBytes(input.ReadableBytes).Array);
                output.Add(obj);
            } catch(Exception e)
            {
                ;
            }
            
        }
    }
}

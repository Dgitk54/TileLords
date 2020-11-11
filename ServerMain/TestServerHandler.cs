using System;
using DotNetty.Common.Utilities;
using DotNetty.Transport.Channels;

namespace Server
{
    public class TestServerHandler : SimpleChannelInboundHandler<object>
    { // (1)

        protected override void ChannelRead0(IChannelHandlerContext context, object message)
        {

            try
            {
                Console.WriteLine(message);
            }
            finally
            {
                ReferenceCountUtil.Release(message);
            }
        }

        public override void ExceptionCaught(IChannelHandlerContext ctx, Exception e)
        {
            Console.WriteLine("{0}", e.ToString());
            ctx.CloseAsync();
        }
    }
}


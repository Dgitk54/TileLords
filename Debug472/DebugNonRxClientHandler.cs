using DataModel.Common.Messages;
using DotNetty.Transport.Channels;

namespace DataModel.Server
{
    public class DebugNonRxClientHandler : SimpleChannelInboundHandler<IMessage>
    {

        
        protected override void ChannelRead0(IChannelHandlerContext ctx, IMessage msg)
        {
            switch (msg)
            {
                case AccountMessage x:
                    if (x.Context == MessageContext.REGISTER)
                    {
                        var tmp = new UserActionMessage() { MessageContext = MessageContext.REGISTER, MessageState = MessageState.SUCCESS };
                        //var tmp = new AccountMessage() { Name = "TESTNAME", Password = "PASSPASS" };
                        ctx.WriteAndFlushAsync(tmp);

                        //context.WriteAsync(tmp);
                    }
                    if (x.Context == MessageContext.LOGIN)
                    {
                        var tmp = new UserActionMessage() { MessageContext = MessageContext.LOGIN, MessageState = MessageState.SUCCESS };
                        //var tmp = new AccountMessage() { Name = "TESTNAME", Password = "PASSPASS" };
                        ctx.WriteAndFlushAsync(tmp);
                        //context.WriteAsync(tmp);
                    }
                    break;
            }
        }

        public override bool IsSharable => true;


    }
}

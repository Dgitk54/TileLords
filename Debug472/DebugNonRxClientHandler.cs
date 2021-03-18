using DataModel.Common;
using DataModel.Common.Messages;
using DotNetty.Buffers;
using DotNetty.Common.Internal.Logging;
using DotNetty.Transport.Channels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DataModel.Server
{
    public class DebugNonRxClientHandler : ChannelHandlerAdapter
    {
        UserActionMessage loginSuccess;
        UserActionMessage registerSuccess;

        public DebugNonRxClientHandler(UserActionMessage registerSuccess, UserActionMessage loginSuccess)
        {
            this.loginSuccess = loginSuccess;
            this.registerSuccess = registerSuccess;
        }
        public override void ChannelActive(IChannelHandlerContext ctx)
        {
            Console.WriteLine("Client Connected");
        }

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            var msg = (IMessage)message;
            switch (msg)
            {
                case AccountMessage x:
                    if (x.Context == MessageContext.REGISTER)
                    {
                        var tmp = new UserActionMessage() { MessageContext = MessageContext.REGISTER, MessageState = MessageState.SUCCESS};
                        //var tmp = new AccountMessage() { Name = "TESTNAME", Password = "PASSPASS" };
                         context.WriteAndFlushAsync(tmp);
                        
                        //context.WriteAsync(tmp);
                    }
                    if (x.Context == MessageContext.LOGIN)
                    {
                        var tmp = new UserActionMessage() { MessageContext = MessageContext.LOGIN, MessageState = MessageState.SUCCESS };
                        //var tmp = new AccountMessage() { Name = "TESTNAME", Password = "PASSPASS" };
                         context.WriteAndFlushAsync(tmp);
                        //context.WriteAsync(tmp);
                    }
                    break;
            }
        }
        public override void ChannelReadComplete(IChannelHandlerContext context)
        {
           // base.ChannelReadComplete(context);
           // context.Flush();
        }
        // The Channel is closed hence the connection is closed
        public override void ChannelInactive(IChannelHandlerContext ctx)
        {

        }


        public override bool IsSharable => true;


    }
}

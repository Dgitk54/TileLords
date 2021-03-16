using DataModel.Common;
using DataModel.Common.Messages;
using DataModel.Server.Model;
using DataModel.Server.Services;
using DotNetty.Buffers;
using DotNetty.Common.Internal.Logging;
using DotNetty.Transport.Channels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DataModel.Server
{
    public class DebugNonRxClientHandler : ChannelHandlerAdapter
    {
        public DebugNonRxClientHandler()
        {

        }
        public override void ChannelActive(IChannelHandlerContext ctx)
        {
            Console.WriteLine("Client Connected");
        }

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            var msg  = (IMessage) message;
            Task.Factory.StartNew(() => 
            {
                switch (msg)
                {
                    case AccountMessage x:
                        if (x.Context == MessageContext.REGISTER)
                        {
                            context.WriteAndFlushAsync(GatewayResponses.registerSuccess);
                        }
                        if (x.Context == MessageContext.LOGIN)
                        {
                            context.WriteAndFlushAsync(GatewayResponses.loginSuccess);
                        }
                        break;
                }
            });
        }

        // The Channel is closed hence the connection is closed
        public override void ChannelInactive(IChannelHandlerContext ctx)
        {

        }


        public override bool IsSharable => true;


    }
}

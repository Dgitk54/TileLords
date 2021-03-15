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

    /// <summary>
    /// Test Debug ClientHandler to measure performance of DotNetty + RX
    /// </summary>
    public class DebugClientHandler : ChannelHandlerAdapter
    {
        readonly Subject<IByteBuffer> clientInboundTraffic = new Subject<IByteBuffer>();
        readonly ISubject<IByteBuffer> synchronizedInboundTraffic;

        public DebugClientHandler()
        {
            synchronizedInboundTraffic = Subject.Synchronize(clientInboundTraffic);
        }

        public override void ChannelActive(IChannelHandlerContext ctx)
        {
            DebugAPIGatewayService.AttachGateWay(clientInboundTraffic).Select(v => v.ToJsonPayload()).Subscribe(v =>
            {
                 ctx.WriteAndFlushAsync(Unpooled.WrappedBuffer(v));
            });
        }

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            var byteBuffer = message as IByteBuffer;
            if (byteBuffer != null)
            {
                TaskPoolScheduler.Default.Schedule(() => clientInboundTraffic.OnNext(byteBuffer));
            }
        }

        // The Channel is closed hence the connection is closed
        public override void ChannelInactive(IChannelHandlerContext ctx)
        {
            Console.WriteLine("Cleaned up ClientHandler");
        }


        public override bool IsSharable => false;
    }
}

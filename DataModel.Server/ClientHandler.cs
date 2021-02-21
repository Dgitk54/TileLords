using DataModel.Common;
using DotNetty.Buffers;
using DotNetty.Common.Internal.Logging;
using DotNetty.Transport.Channels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Joins;
using System.Reactive.Subjects;
using System.Diagnostics;
using System.Reactive.Concurrency;
using LiteDB;
using DataModel.Common.Messages;
using DataModel.Server.Services;

namespace DataModel.Server
{
    public class ClientHandler : ChannelHandlerAdapter
    {
        static readonly IInternalLogger Logger = InternalLoggerFactory.GetInstance<ClientHandler>();

        readonly Subject<IMsgPackMsg> clientInboundTraffic = new Subject<IMsgPackMsg>();
        readonly ISubject<IMsgPackMsg> synchronizedInboundTraffic;
        readonly UserAccountService userAccountService;
        readonly MapContentService mapContentService;
        readonly APIGatewayService apiGatewayService;
        IDisposable responseDisposable;
        public ClientHandler()
        {
            userAccountService = new UserAccountService(DataBaseFunctions.FindUserInDatabase, ServerFunctions.PasswordMatches);
            mapContentService = new MapContentService(DataBaseFunctions.AreaContentRequest, DataBaseFunctions.UpdateOrDeleteContent);
            apiGatewayService = new APIGatewayService(userAccountService, mapContentService);
            synchronizedInboundTraffic = Subject.Synchronize(clientInboundTraffic);
        }

        public override void ChannelActive(IChannelHandlerContext ctx)
        {
            Console.WriteLine("Client connected");
            apiGatewayService.AttachGateway(synchronizedInboundTraffic);

            responseDisposable = apiGatewayService.GatewayResponse.Select(v => v.ToJsonPayload()).Subscribe(v =>
            {
                Console.WriteLine("PUSHING: DATA" + v.GetLength(0));
                ctx.WriteAndFlushAsync(Unpooled.WrappedBuffer(v));
            });
        }

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            ;
            var byteBuffer = message as IByteBuffer;
            if (byteBuffer != null)
            {
                Console.WriteLine("Received" + byteBuffer.ToString(Encoding.UTF8));
                var msgpack = byteBuffer.ToString(Encoding.UTF8).FromString();
                synchronizedInboundTraffic.OnNext(msgpack);
            }
        }

        // The Channel is closed hence the connection is closed
        public override void ChannelInactive(IChannelHandlerContext ctx)
        {
            apiGatewayService.DetachGateway();
            responseDisposable.Dispose();
            Console.WriteLine("Cleaned up ClientHandler");
        }

        public override bool IsSharable => true;
    }


}

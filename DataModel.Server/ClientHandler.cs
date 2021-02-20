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
using MessagePack;
using DataModel.Server.Services;

namespace DataModel.Server
{
    public class ClientHandler : ChannelHandlerAdapter
    {
        static readonly IInternalLogger Logger = InternalLoggerFactory.GetInstance<ClientHandler>();

        readonly Subject<IMsgPackMsg> clientInboundTraffic = new Subject<IMsgPackMsg>();

        readonly UserAccountService userAccountService;
        readonly MapContentService mapContentService;
        readonly APIGatewayService apiGatewayService;
        IDisposable responseDisposable;
        public ClientHandler()
        {
            userAccountService = new UserAccountService(DataBaseFunctions.FindUserInDatabase, ServerFunctions.PasswordMatches);
            mapContentService = new MapContentService(DataBaseFunctions.AreaContentRequest, DataBaseFunctions.UpdateOrDeleteContent);
            apiGatewayService = new APIGatewayService(userAccountService, mapContentService);
        }

        public override void ChannelActive(IChannelHandlerContext ctx)
        {
            Console.WriteLine("Client connected");
            apiGatewayService.AttachGateway(clientInboundTraffic);
            responseDisposable = ServerFunctions.EventStreamSink(apiGatewayService.GatewayResponse, ctx);
        }

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            var byteBuffer = message as IByteBuffer;
            if (byteBuffer != null)
            {
                Console.WriteLine("Received bytes!");
                var lz4Options = MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray);
                IMsgPackMsg data = null;
                try
                {
                    data = MessagePackSerializer.Deserialize<IMsgPackMsg>(byteBuffer.Array);

                }
                catch (MessagePackSerializationException e)
                {
                    Console.WriteLine("Error Deserializing" + e.ToString());
                }
                if (data != null)
                {
                    clientInboundTraffic.Publish(data);
                }
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

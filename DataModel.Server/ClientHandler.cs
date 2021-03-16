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
    public class ClientHandler : ChannelHandlerAdapter
    {
        static readonly IInternalLogger Logger = InternalLoggerFactory.GetInstance<ClientHandler>();

        static bool logInConsole = false;
        readonly Subject<IByteBuffer> clientInboundTraffic = new Subject<IByteBuffer>();
        readonly ISubject<IByteBuffer> synchronizedInboundTraffic;
        readonly UserAccountService userAccountService;
        readonly MapContentService mapContentService;
        readonly ResourceSpawnService resourceSpawnService;
        readonly APIGatewayService apiGatewayService;
        IDisposable responseDisposable;
        public ClientHandler()
        {
            userAccountService = new UserAccountService(MongoDBFunctions.FindUserInDatabase, ServerFunctions.PasswordMatches);
          
            mapContentService = new MapContentService(MongoDBFunctions.AreaContentAsMessageRequest, async (v, e) => await MongoDBFunctions.UpdateOrDeleteContent(v,e));
            resourceSpawnService = new ResourceSpawnService(mapContentService, async (v,e) => await MongoDBFunctions.UpdateOrDeleteContent(v,e), new List<Func<List<MapContent>, bool>>() { ServerFunctions.Only5ResourcesInArea });
            var InventoryService = new InventoryService();
            var questService = new QuestService();
            apiGatewayService = new APIGatewayService(userAccountService, mapContentService, resourceSpawnService, InventoryService, questService);
            synchronizedInboundTraffic = Subject.Synchronize(clientInboundTraffic);
        }

        public override void ChannelActive(IChannelHandlerContext ctx)
        {
            if (logInConsole)
                Console.WriteLine("Client connected");
            apiGatewayService.AttachGateway(synchronizedInboundTraffic.ObserveOn(TaskPoolScheduler.Default).SubscribeOn(TaskPoolScheduler.Default));

            responseDisposable = apiGatewayService.GatewayResponse.Do(v => { if (logInConsole) { Console.WriteLine(v); } }).ObserveOn(TaskPoolScheduler.Default).Select(v => v.ToJsonPayload()).Select(v=> Unpooled.WrappedBuffer(v)).Subscribe(v =>
            {
                TaskPoolScheduler.Default.Schedule(() => ctx.WriteAndFlushAsync(Unpooled.WrappedBuffer(v)));
            });
        }

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            var byteBuffer = message as IByteBuffer;
            if (byteBuffer != null)
            {
                if (logInConsole)
                    Console.WriteLine("Received" + byteBuffer.ToString(Encoding.UTF8));
                TaskPoolScheduler.Default.Schedule(() => synchronizedInboundTraffic.OnNext(byteBuffer));
            }
        }
       
        // The Channel is closed hence the connection is closed
        public override void ChannelInactive(IChannelHandlerContext ctx)
        {
            apiGatewayService.DetachGateway();
            responseDisposable.Dispose();
            if (logInConsole)
                Console.WriteLine("Cleaned up ClientHandler");
        }

        public override bool IsSharable => false;
    }


}

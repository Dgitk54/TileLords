using DataModel.Common;
using DataModel.Common.Messages;
using DataModel.Server.Model;
using DataModel.Server.Services;
using DotNetty.Buffers;
using DotNetty.Common.Internal.Logging;
using DotNetty.Transport.Channels;
using MessagePack;
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

        static bool logInConsole = false;
        readonly Subject<byte[]> clientInboundTraffic = new Subject<byte[]>();
        readonly ISubject<byte[]> synchronizedInboundTraffic;
        readonly UserAccountService userAccountService;
        readonly MapContentService mapContentService;
        readonly ResourceSpawnService resourceSpawnService;
        readonly APIGatewayService apiGatewayService;
        IDisposable responseDisposable;
        readonly TaskFactory scheduler;
        readonly MessagePackSerializerOptions lz4Options;
        
        public ClientHandler(TaskFactory scheduler, ref MessagePackSerializerOptions options)
        {
            userAccountService = new UserAccountService(LiteDBDatabaseFunctions.FindUserInDatabase, ServerFunctions.PasswordMatches);
            mapContentService = new MapContentService();
            resourceSpawnService = new ResourceSpawnService(mapContentService, LiteDBDatabaseFunctions.UpsertOrDeleteContent, new List<Func<List<MapContent>, bool>>() { ServerFunctions.Only5ResourcesInArea });
            var InventoryService = new InventoryService();
            var questService = new QuestService();
            apiGatewayService = new APIGatewayService(userAccountService, mapContentService, resourceSpawnService, InventoryService, questService, ref options);
            this.scheduler = scheduler;
            synchronizedInboundTraffic = Subject.Synchronize(clientInboundTraffic);
            lz4Options = options;
        }

        public override void ChannelActive(IChannelHandlerContext ctx)
        {
            if (logInConsole)
                Console.WriteLine("Client connected");
            apiGatewayService.AttachGateway(synchronizedInboundTraffic);

            responseDisposable = apiGatewayService.GatewayResponse.Do(v => { if (logInConsole) { Console.WriteLine("GATEWAYRESPONSEOUTPUT" + v); } })
                                                                  .Select(v=>
                                                                  {
                                                                     return Observable.Start(() => MessagePackSerializer.Serialize(v, lz4Options));
                                                                  })
                                                                  .SelectMany(v=> v)
                                                                  .Select(v=> Unpooled.WrappedBuffer(v))
                                                                  .Subscribe(v =>
                                                                  {
                                                                      ctx.WriteAndFlushAsync(v);
                                                                  });
        }

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            scheduler.StartNew(() =>
            {
                var castedMessage = (IByteBuffer)message;
                int length = castedMessage.ReadableBytes;
                var array = new byte[length];
                castedMessage.GetBytes(castedMessage.ReaderIndex, array, 0, length);
                synchronizedInboundTraffic.OnNext(array);
            });
        }

        public override void ChannelInactive(IChannelHandlerContext ctx)
        {
            apiGatewayService.DetachGateway();
            responseDisposable.Dispose();
            if (logInConsole)
                Console.WriteLine("Cleaned up ClientHandler");
        }

        public override bool IsSharable => true;
    }


}

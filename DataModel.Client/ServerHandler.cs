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
using Newtonsoft.Json;
using System.Reactive.Concurrency;

namespace DataModel.Client
{
    public class ServerHandler : ChannelHandlerAdapter
    {



        static readonly IInternalLogger Logger = InternalLoggerFactory.GetInstance<ServerHandler>();
        readonly List<IDisposable> disposables = new List<IDisposable>();
        readonly IEventBus eventBus;
        




        public ServerHandler(IEventBus bus) => eventBus = bus;


        public override void ChannelActive(IChannelHandlerContext context)
        {
            eventBus.Publish(new ClientConnectedEvent("Connected on " + DateTime.Now.ToString()));
            disposables.Add(ClientFunctions.EventStreamSink(eventBus.GetEventStream<DataSinkEvent>(), context));
            disposables.Add(new ClientGPSHandler(eventBus).AttachToBus());
            disposables.Add(new ClientMapBufferHandler(eventBus).AttachToBus());
        //    disposables.Add(new MapAsRenderableForUnityHandler(eventBus).AttachToBus());
            disposables.Add(new ClientActionSuccessHandler(eventBus).AttachToBus());
            disposables.Add(new ClientActionErrorHandler(eventBus).AttachToBus());
        }


        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            var byteBuffer = message as IByteBuffer;
            if (byteBuffer != null)
            {
                var data = byteBuffer.ToString(Encoding.UTF8);
                //   if (data.Length > 500)
                //   {
                //       Console.WriteLine("Received large" + data.Length);
                //   } else
                //   {
                //       Console.WriteLine(data);
                //   }
                eventBus.Publish(new DataSourceEvent(data));
                
            }

        }

        public override void ChannelInactive(IChannelHandlerContext ctx)
        {
            eventBus.Publish(new ClientDisconnectedEvent("DC on " + DateTime.Now.ToString()));
        }

        public void ShutDown() => disposables.ForEach(v => v.Dispose());


        public override bool IsSharable => true;
    }


}
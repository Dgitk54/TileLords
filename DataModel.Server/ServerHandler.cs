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

namespace DataModel.Server
{
    public class ServerHandler : ChannelHandlerAdapter
    {
        static readonly IInternalLogger Logger = InternalLoggerFactory.GetInstance<ServerHandler>();

        private readonly Subject<string> jsonClientSource = new Subject<string>();
        private readonly ServerFunctions functions = new ServerFunctions();
        private List<IDisposable> sinks = new List<IDisposable>();
        
        public override void ChannelActive(IChannelHandlerContext ctx)
        {
            Console.WriteLine("ClientHandler active");
            var jsonMessages = functions.TransformPacket(jsonClientSource);
            var gpsUpdates = functions.GPSMessageStream(jsonMessages);
            var clientPlusCodeStream = functions.GpsAsPlusCode8(gpsUpdates);
            var clientChangedTile = functions.TileHasChangedStream(clientPlusCodeStream);
            var clientTileUpdate = functions.TilesForPlusCode(clientChangedTile);


            const int periodInSec = 2;
            var obs = Observable.Interval(TimeSpan.FromSeconds(periodInSec),
                                          Scheduler.Default);


            sinks.Add(functions.StreamSink<PlusCode>(clientChangedTile, ctx));

            //TODO: Why does this sink not work?
            sinks.Add(functions.StreamSink<List<Tile>>(clientTileUpdate, ctx));

            sinks.Add(functions.StreamSink<long>(obs, ctx));


            // Detect when client disconnects
            ctx.Channel.CloseCompletion.ContinueWith((x) => Console.WriteLine("Channel Closed"));
            

        }


        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            var byteBuffer = message as IByteBuffer;
            if (byteBuffer != null)
            {
                Console.WriteLine("Received from client: " + byteBuffer.ToString(Encoding.UTF8));
                jsonClientSource.OnNext(byteBuffer.ToString(Encoding.UTF8));
                
            }
            //context.WriteAsync(message);
            
        }

        // The Channel is closed hence the connection is closed
        public override void ChannelInactive(IChannelHandlerContext ctx)
        {
            Console.WriteLine("Client disconnected");
            jsonClientSource.OnCompleted();
            sinks.ForEach(v => v.Dispose());
        }

        

        public override bool IsSharable => true;
    }

   
}

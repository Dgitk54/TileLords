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

namespace DataModel.Server
{
    public class ClientHandler : ChannelHandlerAdapter
    {
        static readonly IInternalLogger Logger = InternalLoggerFactory.GetInstance<ClientHandler>();

        private readonly Subject<byte[]> byteStream;
        private readonly ServerFunctions functions = new ServerFunctions();
        
        public override void ChannelActive(IChannelHandlerContext ctx)
        {
            var jsonMessages = functions.TransformPacket(byteStream);
            var gpsUpdates = functions.GPSMessageStream(jsonMessages);
            var clientPlusCodeStream = functions.GpsAsPlusCode8(gpsUpdates);
            var clientChangedTile = functions.TileHasChangedStream(clientPlusCodeStream);
            var clientTileUpdate = functions.TilesForPlusCode(clientChangedTile);
            var encodeTileUpdate = functions.EncodeTileUpdate(clientTileUpdate);


            
            functions.AddClientCallBack<NetworkJsonMessage>(encodeTileUpdate, ctx);
            
           
            // Detect when client disconnects
            ctx.Channel.CloseCompletion.ContinueWith((x) => Console.WriteLine("Channel Closed"));
            

        }


        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            var byteBuffer = message as IByteBuffer;
            if (byteBuffer != null)
            {
                byteStream.OnNext(byteBuffer.Array);
                Console.WriteLine("Received from server: " + byteBuffer.ToString(Encoding.UTF8));
            }
            context.WriteAsync(message);
            
        }

        // The Channel is closed hence the connection is closed
        public override void ChannelInactive(IChannelHandlerContext ctx)
        {
            Console.WriteLine("Client disconnected");
            byteStream.Dispose();
        }

        

        public override bool IsSharable => true;
    }

   
}

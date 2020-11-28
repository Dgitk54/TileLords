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

namespace DataModel.Client
{
    public class ClientHandler : ChannelHandlerAdapter
    {
        static readonly IInternalLogger Logger = InternalLoggerFactory.GetInstance<ClientHandler>();

        List<GPS> gpsList = new List<GPS>();

        public ClientHandler()
        {
            gpsList.Add(new GPS(49.944365, 7.919616));
            gpsList.Add(new GPS(49.944366, 7.919616));
            gpsList.Add(new GPS(49.944366, 7.919617));
            gpsList.Add(new GPS(52.519126, 13.406101));
        }

        public override void ChannelActive(IChannelHandlerContext context)
        {

            ClientFunctions.StreamSink<GPS>(gpsList.ToObservable(), context, 1024);


            // Detect when server disconnects
            context.Channel.CloseCompletion.ContinueWith((x) => Console.WriteLine("Channel Closed"));


        }


        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            var byteBuffer = message as IByteBuffer;
            if (byteBuffer != null)
            {
                Console.WriteLine("Received from server: " + byteBuffer.ToString(Encoding.UTF8));
            }
           // context.WriteAsync(message);

        }

        // The Channel is closed hence the connection is closed
        public override void ChannelInactive(IChannelHandlerContext ctx)
        {
            Console.WriteLine("Client shut down");
        }



        public override bool IsSharable => true;
    }


}
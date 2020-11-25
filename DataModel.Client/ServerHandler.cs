﻿using DataModel.Common;
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
    public class ServerHandler : ChannelHandlerAdapter
    {
        static readonly IInternalLogger Logger = InternalLoggerFactory.GetInstance<ServerHandler>();



        public override void ChannelActive(IChannelHandlerContext ctx)
        {


            List<GPS> gpsList = new List<GPS>();
            gpsList.Add(new GPS(49.944365, 7.919616));
            gpsList.Add(new GPS(52.519126, 13.406101));

            var disp = gpsList.ToObservable().Subscribe(v => 
            { 
                var sendstring = JsonConvert.SerializeObject(v);
                Console.WriteLine("Send to server: " + sendstring);
                ctx.WriteAndFlushAsync(ByteBufferUtil.EncodeString(ctx.Allocator, JsonConvert.SerializeObject(new NetworkJsonMessage(sendstring)), Encoding.ASCII)).Wait(); 
             } );


            // Detect when client disconnects
            ctx.Channel.CloseCompletion.ContinueWith((x) => Console.WriteLine("Channel Closed"));

            disp.Dispose();
        }


        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            var byteBuffer = message as IByteBuffer;
            if (byteBuffer != null)
            {
                
                Console.WriteLine("Received from server: " + byteBuffer.ToString(Encoding.UTF8));
            }
            context.WriteAsync(message);

        }

        // The Channel is closed hence the connection is closed
        public override void ChannelInactive(IChannelHandlerContext ctx)
        {
            Console.WriteLine("Client disconnected");
        }



        public override bool IsSharable => true;
    }


}
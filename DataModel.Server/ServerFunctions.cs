using DataModel.Common;
using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Joins;
using Google.OpenLocationCode;
using DotNetty.Transport.Channels;
using System.Diagnostics;
using DotNetty.Buffers;

namespace DataModel.Server
{
    /// <summary>
    /// Class with functions shared between multiple handlers.
    /// </summary>
    public class ServerFunctions
    {

        /*
           from jsonCode in (from bytePacket in dataStream
                            select JsonConvert.ToString(bytePacket))
            select new NetworkJsonMessage(jsonCode);*/



        public static IDisposable DebugEventToConsoleSink<T>(IObservable<T> events) where T : IEvent
            => events.Subscribe(v => Console.WriteLine("Event occured:" + v.ToString()));

        public static IDisposable EventStreamSink<T>(IObservable<T> objStream, IChannelHandlerContext context) where T : DataSinkEvent
            => objStream.Subscribe(v =>
            {
                var asByteMessage = Encoding.UTF8.GetBytes(v.SerializedData);
                Console.WriteLine("PUSHING: DATA" + asByteMessage.GetLength(0));
                context.WriteAndFlushAsync(Unpooled.WrappedBuffer(asByteMessage));
            },
             e => Console.WriteLine("Error occured writing" + objStream),
             () => Console.WriteLine("StreamSink Write Sequence Completed"));

       


    }
}

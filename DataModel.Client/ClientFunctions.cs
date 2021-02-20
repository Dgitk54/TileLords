using DataModel.Common;
using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Reactive.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using MessagePack;
using DataModel.Common.Messages;

namespace DataModel.Client
{
    /// <summary>
    /// Class for functions shared between multiple handlers.
    /// </summary>
    public static class ClientFunctions
    {
        public static IDisposable EventStreamSink<T>(IObservable<T> objStream, IChannelHandlerContext context) where T : IMsgPackMsg
            => objStream.Subscribe(v =>
            {
                //var lz4Options = MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray);
                var data = MessagePackSerializer.Serialize(v);
                Console.WriteLine("PUSHING: DATA" + data.GetLength(0));
                context.WriteAndFlushAsync(Unpooled.WrappedBuffer(data));
            },
             e => Console.WriteLine("Error occured writing" + objStream),
             () => Console.WriteLine("StreamSink Write Sequence Completed"));

    }
}

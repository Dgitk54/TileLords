using DataModel.Common;
using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Reactive.Linq;
using Newtonsoft.Json;
namespace DataModel.Client
{
    /// <summary>
    /// Class for functions shared between multiple handlers.
    /// </summary>
    public static class ClientFunctions
    {
        public static IDisposable EventStreamSink<T>(IObservable<T> objStream, IChannelHandlerContext context) where T : DataSinkEvent
        => objStream.Subscribe(v =>
        {
            var asByteMessage = Encoding.UTF8.GetBytes(v.SerializedData);
            Debug.WriteLine("CLIENT PUSHING: DATA" + asByteMessage.GetLength(0));
            context.WriteAndFlushAsync(Unpooled.WrappedBuffer(asByteMessage));
        },
         e => Debug.WriteLine("Error occured writing" + objStream),
         () => Debug.WriteLine("StreamSink Write Sequence Completed"));

        public static IDisposable DebugEventToConsoleSink<T>(IObservable<T> events) where T : IEvent
            => events.Subscribe(v => Console.WriteLine("Event occured:" + v.ToString()));

        

        public static IObservable<PlusCode> LatestClientLocation(IObservable<UserGpsChangedEvent> observable) => from e in observable
                                                                                                                   select DataModelFunctions.GetPlusCode(e.GpsData, 10);
    }
}

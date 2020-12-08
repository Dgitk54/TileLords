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

        

        public static IObservable<PlusCode> LatestClientLocation(IObservable<UserGpsEvent> observable) => from e in observable
                                                                                                                   select DataModelFunctions.GetPlusCode(e.GpsData, 10);

        public static IObservable<T> ParseOnlyValidUsingErrorHandler<T>(IObservable<DataSourceEvent> observable, EventHandler<ErrorEventArgs> eventHandler) where T : IEvent

        {
            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                MissingMemberHandling = MissingMemberHandling.Error,
                Error = eventHandler,
                NullValueHandling = NullValueHandling.Ignore
            };
            if (eventHandler == null)
                throw new Exception("Eventhandler is null!");

            var rawData = from e in observable
                          select e.Data;
            var parseDataIgnoringErrors = from e in rawData
                                          select JsonConvert.DeserializeObject<T>(e, settings);

            return from e in parseDataIgnoringErrors
                   where e != null
                   select e;

        }
        public static void PrintConsoleErrorHandler(object sender, ErrorEventArgs errorArgs)
        {
            var currentError = errorArgs.ErrorContext.Error.Message;
            Console.WriteLine(currentError);
            errorArgs.ErrorContext.Handled = true;
        }
    }
}

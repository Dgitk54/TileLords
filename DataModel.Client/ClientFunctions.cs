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
using System.Threading.Tasks;
using System.Threading;

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
        public static Task StartClient(ClientInstance instance)
        {
            var waitForConnection = Task.Run(() =>
            {
                var result = instance.ClientConnectionState.Do(v => Console.WriteLine(v)).Where(v => v).Take(1)
                            .Timeout(DateTime.Now.AddSeconds(5)).Wait();
                return result;
            });
            Thread.Sleep(300);
            var run = Task.Run(() => instance.RunClientAsyncWithIP());
            waitForConnection.Wait();
            return run;
        }
        public static void SendGpsPath(ClientInstance instance, CancellationToken ct, List<GPS> gps, int sleeptime)
        {
            int i = 0;
            do
            {
                instance.SendGps(gps[i % gps.Count]);
                Thread.Sleep(sleeptime);
                i++;
                if (ct.IsCancellationRequested)
                    break;

            } while (!ct.IsCancellationRequested);
        }
    }
}

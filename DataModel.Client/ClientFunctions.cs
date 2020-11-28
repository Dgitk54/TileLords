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
    public static class ClientFunctions
    {

        public static void StreamSink<T>(IObservable<T> obj, IChannelHandlerContext context, int bufferSize)
        {
            
            obj.Subscribe(v => 
            {
                var insideBuffer = Unpooled.Buffer(bufferSize);
                var asStringPayload = JsonConvert.SerializeObject(v);
                var asByteMessage = Encoding.UTF8.GetBytes(asStringPayload);
               
                insideBuffer.WriteBytes(asByteMessage);
                context.WriteAndFlushAsync(insideBuffer);
            }, 
            e => Debug.WriteLine("Error occured writing" + obj), 
            () => Debug.WriteLine("Write Sequence Completed"));
        }

        public static void AddStreamCallBack<T>(IObservable<T> obj, IChannelHandlerContext context) => obj.Subscribe(v => context.WriteAndFlushAsync(v), e => Debug.WriteLine("Error occured writing" + obj), () => Debug.WriteLine("Write Sequence Completed"));



    }
}

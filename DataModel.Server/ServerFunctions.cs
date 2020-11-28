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
    public class ServerFunctions
    {

        /*
           from jsonCode in (from bytePacket in dataStream
                            select JsonConvert.ToString(bytePacket))
            select new NetworkJsonMessage(jsonCode);*/

        public IObservable<NetworkJsonMessage> TransformPacket(IObservable<string> dataStream) => from message in dataStream
                                                                                                  select new NetworkJsonMessage(message);

        public IObservable<GPS> GPSMessageStream(IObservable<NetworkJsonMessage> message) => from msg in message
                                                                                             select JsonConvert.DeserializeObject<GPS>(msg.JsonPayload);


        public IObservable<PlusCode> GetPlusCode(IObservable<GPS> gps, IObservable<int> precision)
            => from i in gps
               from j in precision
               select new PlusCode(new OpenLocationCode(i.Lat, i.Lon, j).Code, j);

        public IObservable<PlusCode> GpsAsPlusCode8(IObservable<GPS> gpsStream) => GetPlusCode(gpsStream, Observable.Create<int>(v => { v.OnNext(8); return v.OnCompleted; }));


        public IObservable<PlusCode> TileHasChangedStream(IObservable<PlusCode> plusCodeStream) => plusCodeStream.DistinctUntilChanged();


        //TODO: Add persistent storage/database access, currently only for small milestone/debugging
        public IObservable<List<Tile>> TilesForPlusCode(IObservable<PlusCode> code) => from val in code
                                                                                       select TileGenerator.GenerateArea(val, 1);
        public IObservable<Tile> EachTileSeperate(IObservable<List<Tile>> observable) => from list in observable
                                                                                      from tile in list.ToObservable()
                                                                                      select tile;


        public IObservable<NetworkJsonMessage> EncodeTileUpdate(IObservable<List<Tile>> tileStream) => from tileList in tileStream
                                                                                                       let encoded = JsonConvert.SerializeObject(tileList)
                                                                                                       select new NetworkJsonMessage(encoded);





        public IDisposable StreamSink<T>(IObservable<T> obj, IChannelHandlerContext context)
        {

            return obj.Subscribe(v =>
             {
                 var asStringPayload = JsonConvert.SerializeObject(v);
                 var asByteMessage = Encoding.UTF8.GetBytes(asStringPayload);
                 Console.WriteLine("PUSHING: DATA" + asByteMessage.GetLength(0));
                 context.WriteAndFlushAsync(Unpooled.WrappedBuffer(asByteMessage));

             },
             e => Console.WriteLine("Error occured writing" + obj),
             () => Console.WriteLine("StreamSink Write Sequence Completed"));
        }


    }
}

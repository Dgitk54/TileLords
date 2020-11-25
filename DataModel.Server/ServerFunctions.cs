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

namespace DataModel.Server
{
    public class ServerFunctions
    {
        public IObservable<NetworkJsonMessage> TransformPacket(IObservable<byte[]> dataStream) => from jsonCode in (from bytePacket in dataStream
                                                                                                                    select JsonConvert.ToString(bytePacket))
                                                                                                  select new NetworkJsonMessage(jsonCode);

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

        public IObservable<NetworkJsonMessage> EncodeTileUpdate(IObservable<List<Tile>> tileStream) => from tileList in tileStream
                                                                                                       let encoded = JsonConvert.SerializeObject(tileList)
                                                                                                       select new NetworkJsonMessage(encoded);


        public void AddClientCallBack<T>(IObservable<T> obj, IChannelHandlerContext context) => obj.Subscribe(v => context.WriteAndFlushAsync(v), e => Debug.WriteLine("Error occured writing" + obj), () => Debug.WriteLine("Write Sequence Completed"));




    }
}

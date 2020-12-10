using DataModel.Common;
using Google.OpenLocationCode;
using LiteDB;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;

namespace DataModel.Server
{
    /// <summary>
    /// Handles gps data received from the client.
    /// </summary>
    public class ClientChunkupdateHandler
    {
        readonly IEventBus cEventBus;
        readonly ILiteDatabase dataBase;
        readonly JsonSerializerSettings settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.None
        };
        public ClientChunkupdateHandler(IEventBus clientBus, ILiteDatabase db)
        {
            cEventBus = clientBus;
            dataBase = db;
        }
        public IDisposable AttachToBus()
        {
            var onlyValid = ServerFunctions.ParseOnlyValidUsingErrorHandler<UserGpsEvent>(cEventBus.GetEventStream<DataSourceEvent>(), ServerFunctions.PrintConsoleErrorHandler);

            var onlyNonDefault = from e in onlyValid
                                 where !e.GpsData.Equals(default)
                                 select e;



            var createResponse = from v in TilesForPlusCode(TileHasChangedStream(GPSAsPluscode8(GpsFromClientGpsEvent(onlyNonDefault))))
                                 let e = v.GetServerMapEvent()
                                 let serialized = JsonConvert.SerializeObject(e, typeof(ServerMapEvent), settings)
                                 select new DataSinkEvent(serialized);

            return createResponse.Subscribe(v => cEventBus.Publish(v));
        }



        IObservable<GPS> GpsFromClientGpsEvent(IObservable<UserGpsEvent> observable) => from e in observable
                                                                                        select e.GpsData;



        IObservable<PlusCode> GPSAsPluscode8(IObservable<GPS> gpsStream) => DataModelFunctions.GetPlusCode(gpsStream, Observable.Create<int>(v => { v.OnNext(8); return v.OnCompleted; }));


        IObservable<PlusCode> TileHasChangedStream(IObservable<PlusCode> plusCodeStream) => plusCodeStream.DistinctUntilChanged();


        IObservable<Tile> TilesForPlusCode(IObservable<PlusCode> code)
        {
            var stream = from val in code
                         let neighbors = ServerFunctions.NeighborsIn8(val)
                         from n in neighbors
                         select ServerFunctions.LookUpTile(n, dataBase);
            return stream;

        }



    }
}

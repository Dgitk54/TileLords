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
    public class ClientLocationHandler
    {
        readonly IEventBus cEventBus;
        readonly ILiteDatabase dataBase;
        public ClientLocationHandler(IEventBus clientBus, ILiteDatabase db)
        {
            cEventBus = clientBus;
            dataBase = db;
        }
        public IDisposable AttachToBus()
        {
            var toSink = from v in TilesForPlusCode(TileHasChangedStream(GPSAsPluscode8(GpsFromClientGpsEvent(ParseOnlyValidIgnoringErrors(cEventBus.GetEventStream<DataSourceEvent>())))))
                         select new DataSinkEvent(JsonConvert.SerializeObject(v));
            return toSink.Subscribe(v => cEventBus.Publish(v));
        }


        IObservable<UserGpsChangedEvent> ParseOnlyValidIgnoringErrors(IObservable<DataSourceEvent> observable)
        {
            var rawData = from e in observable
                          select e.Data;
            var parseDataIgnoringErrors = from e in rawData
                                          select JsonConvert.DeserializeObject<UserGpsChangedEvent>(e, new JsonSerializerSettings
                                          {
                                              Error = HandleDeserializationError
                                          });
            return from e in parseDataIgnoringErrors
                   where e != null
                   select e;

        }


        IObservable<GPS> GpsFromClientGpsEvent(IObservable<UserGpsChangedEvent> observable) => from e in observable
                                                                                                 select e.GpsData;


        public void HandleDeserializationError(object sender, ErrorEventArgs errorArgs)
        {
            var currentError = errorArgs.ErrorContext.Error.Message;
            Console.WriteLine(currentError);
            errorArgs.ErrorContext.Handled = true;
        }


        IObservable<PlusCode> GPSAsPluscode8(IObservable<GPS> gpsStream) => DataModelFunctions.GetPlusCode(gpsStream, Observable.Create<int>(v => { v.OnNext(8); return v.OnCompleted; }));


        IObservable<PlusCode> TileHasChangedStream(IObservable<PlusCode> plusCodeStream) => plusCodeStream.DistinctUntilChanged();


        IObservable<Tile> TilesForPlusCode(IObservable<PlusCode> code)
        {
            var stream = from val in code
                         let neighbors = ServerFunctions.NeighborsIn8(val)
                         from n in neighbors
                         select ServerFunctions.LookUp(n, dataBase);
            return stream;

        }

    }
}

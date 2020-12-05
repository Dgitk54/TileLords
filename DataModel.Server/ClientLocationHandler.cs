using DataModel.Common;
using Google.OpenLocationCode;
using LiteDB;
using Newtonsoft.Json;
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
            var dataSinks = from v in TilesForPlusCode(TileHasChangedStream(GPSAsPluscode8(GpsFromClientGpsEvent(GPSMessageStream(DataExtractor(cEventBus.GetEventStream<DataSourceEvent>()))))))
                            select new DataSinkEvent(JsonConvert.SerializeObject(v));


            return dataSinks.Subscribe(v => cEventBus.Publish<DataSinkEvent>(v));
        }

        IObservable<string> DataExtractor(IObservable<DataSourceEvent> eventSource) => from e in eventSource
                                                                                       select e.Data;

        IObservable<GPS> GpsFromClientGpsEvent(IObservable<ClientGpsChangedEvent> observable) => from e in observable
                                                                                                 select e.ClientGPSHasChanged;

        IObservable<ClientGpsChangedEvent> GPSMessageStream(IObservable<string> message) => from msg in message
                                                                                            select JsonConvert.DeserializeObject<ClientGpsChangedEvent>(msg);

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
        IObservable<Tile> EachTileSeperate(IObservable<IList<Tile>> observable) => from list in observable
                                                                                  from tile in list.ToObservable()
                                                                                  select tile;
    }
}

using DataModel.Common;
using Google.OpenLocationCode;
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
        private IEventBus ClientBus { get; }
        public ClientLocationHandler(IEventBus clientBus)
        {
            ClientBus = clientBus;
        }
        public IDisposable AttachToBus()
        {
            var dataSinks = from v in EachTileSeperate(TilesForPlusCode(TileHasChangedStream(GPSAsPluscode8(GpsFromClientGpsEvent(GPSMessageStream(DataExtractor(ClientBus.GetEventStream<DataSourceEvent>())))))))
                            select new DataSinkEvent(JsonConvert.SerializeObject(v));
            return dataSinks.Subscribe(v => ClientBus.Publish<DataSinkEvent>(v));
        }

        IObservable<string> DataExtractor(IObservable<DataSourceEvent> eventSource) => from e in eventSource
                                                                                       select e.Data;

        IObservable<GPS> GpsFromClientGpsEvent(IObservable<ClientGpsChangedEvent> observable) => from e in observable
                                                                                                 select e.ClientGPSHasChanged;

        IObservable<ClientGpsChangedEvent> GPSMessageStream(IObservable<string> message) => from msg in message
                                                                                            select JsonConvert.DeserializeObject<ClientGpsChangedEvent>(msg);

        IObservable<PlusCode> GPSAsPluscode8(IObservable<GPS> gpsStream) => DataModelFunctions.GetPlusCode(gpsStream, Observable.Create<int>(v => { v.OnNext(8); return v.OnCompleted; }));


        IObservable<PlusCode> TileHasChangedStream(IObservable<PlusCode> plusCodeStream) => plusCodeStream.DistinctUntilChanged();


        IObservable<List<Tile>> TilesForPlusCode(IObservable<PlusCode> code) => from val in code
                                                                                select TileGenerator.GenerateArea(val, 1);
        IObservable<Tile> EachTileSeperate(IObservable<List<Tile>> observable) => from list in observable
                                                                                  from tile in list.ToObservable()
                                                                                  select tile;
    }
}

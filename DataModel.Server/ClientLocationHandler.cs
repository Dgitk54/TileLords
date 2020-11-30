using DataModel.Common;
using Google.OpenLocationCode;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;

namespace DataModel.Server
{

    public class ClientLocationHandler
    {
        private IEventBus ClientBus { get; }
        public ClientLocationHandler(IEventBus clientBus)
        {
            ClientBus = clientBus;
            

        }
        public IDisposable AttachToBus()
        {
            var dataSinks = from v in EachTileSeperate(TilesForPlusCode(TileHasChangedStream(GPSAsPluscode8(GPSMessageStream(DataExtractor(ClientBus.GetEventStream<DataSourceEvent>()))))))
                            select new DataSinkEvent(JsonConvert.SerializeObject(v));
            return dataSinks.Subscribe(v => ClientBus.Publish<DataSinkEvent>(v));
        }

        IObservable<string> DataExtractor(IObservable<DataSourceEvent> eventSource) => from e in eventSource
                                                                                       select e.Data;
        IObservable < GPS > GPSMessageStream(IObservable<string> message) => from msg in message
                                                                          select JsonConvert.DeserializeObject<GPS>(msg);


        IObservable<PlusCode> GetPlusCode(IObservable<GPS> gps, IObservable<int> precision)
            => from i in gps
               from j in precision
               select new PlusCode(new OpenLocationCode(i.Lat, i.Lon, j).Code, j);

        IObservable<PlusCode> GPSAsPluscode8(IObservable<GPS> gpsStream) => GetPlusCode(gpsStream, Observable.Create<int>(v => { v.OnNext(8); return v.OnCompleted; }));


        IObservable<PlusCode> TileHasChangedStream(IObservable<PlusCode> plusCodeStream) => plusCodeStream.DistinctUntilChanged();


        IObservable<List<Tile>> TilesForPlusCode(IObservable<PlusCode> code) => from val in code
                                                                                select TileGenerator.GenerateArea(val, 1);
        IObservable<Tile> EachTileSeperate(IObservable<List<Tile>> observable) => from list in observable
                                                                                  from tile in list.ToObservable()
                                                                                  select tile;
    }
}

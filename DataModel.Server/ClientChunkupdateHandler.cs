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
    public class ClientChunkUpdateHandler
    {
        readonly IEventBus cEventBus;
        readonly ILiteDatabase dataBase;
        readonly JsonSerializerSettings settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.None
        };
        public ClientChunkUpdateHandler(IEventBus clientBus, ILiteDatabase db)
        {
            cEventBus = clientBus;
            dataBase = db;
        }
        public IDisposable AttachToBus()
        {
            var createResponse = from v in TilesForPlusCode(TileHasChangedStream(ServerFunctions.ExtractPlusCodeLocationStream(cEventBus, 8)))
                                 let e = v.GetServerMapEvent()
                                 let serialized = JsonConvert.SerializeObject(e, typeof(ServerMapEvent), settings)
                                 select new DataSinkEvent(serialized);

            return createResponse.Subscribe(v => cEventBus.Publish(v));
        }


       
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

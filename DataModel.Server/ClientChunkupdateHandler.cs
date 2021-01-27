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
        readonly IMessageBus cEventBus;
        readonly JsonSerializerSettings settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.None
        };
        public ClientChunkUpdateHandler(IMessageBus clientBus)
        {
            cEventBus = clientBus;
        }
        public IDisposable AttachToBus()
        {
            var createResponse = from v in TilesForPlusCode(TileHasChangedStream(ServerFunctions.ExtractPlusCodeLocationStream(cEventBus, 8)))
                                 let e = v.GetServerMapEvent()
                                 let serialized = JsonConvert.SerializeObject(e, typeof(ServerMapEvent), settings)
                                 let servertilecontent = GrabContentFromTile(v)
                                 select (new DataSinkEvent(serialized), servertilecontent);

            return createResponse.Subscribe(v => 
            {
               // Console.WriteLine("PUSHING" + DateTime.Now);
               // cEventBus.Publish(v.Item1);
               // cEventBus.Publish(v.servertilecontent);
            });
        }

        ServerTileContentEvent GrabContentFromTile(Tile t)
        {
            var ret = new ServerTileContentEvent();
            
            var staticContentUpdate = new List<KeyValuePair<PlusCode, List<ITileContent>>> ();

            t.MiniTiles.ForEach(v => 
            {
                staticContentUpdate.Add(new KeyValuePair<PlusCode, List<ITileContent>>(v.MiniTileId, v.Content)  );
            });

            ret.VisibleContent = staticContentUpdate;
            return ret; 
        }

       
        IObservable<PlusCode> TileHasChangedStream(IObservable<PlusCode> plusCodeStream) => plusCodeStream.DistinctUntilChanged();


        IObservable<Tile> TilesForPlusCode(IObservable<PlusCode> code)
        {
            var stream = from val in code
                         let neighbors = ServerFunctions.NeighborsIn8(val)
                         from n in neighbors
                         select DataBaseFunctions.LookUpWithGenerateTile(n);
            return stream;

        }



    }
}

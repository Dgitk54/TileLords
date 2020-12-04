using DataModel.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Reactive.Linq;
using System.Diagnostics;

namespace DataModel.Client
{
    public class ClientMapBufferHandler
    {
        readonly IEventBus eventBus;
        public ClientMapBufferHandler(IEventBus bus) => eventBus = bus;

        public IDisposable AttachToBus()
        {
            var receivedLarge = BufferMiniTiles(AsMiniTiles(LargeTileUpdate(eventBus.GetEventStream<DataSourceEvent>())));
            var receivedSmall = SmallUpdate(MiniTileUpdate(eventBus.GetEventStream<DataSourceEvent>()));
            var latestClient = ClientFunctions.LatestClientLocation(eventBus.GetEventStream<ClientGpsChangedEvent>());

            return MapBufferChanged(receivedLarge, receivedSmall, latestClient)
                .Subscribe(v => eventBus.Publish<ClientMapBufferChanged>(new ClientMapBufferChanged(v)));
        }





        IObservable<IList<MiniTile>> MapBufferChanged(IObservable<IList<MiniTile>> observable1, IObservable<MiniTile> observable2, IObservable<PlusCode> location)
        => Accumulated(observable1, location);

        IObservable<IList<MiniTile>> BufferMiniTiles(IObservable<MiniTile> observable) => observable.Buffer(TimeSpan.FromSeconds(2));


        IObservable<IList<MiniTile>> Accumulated(IObservable<IList<MiniTile>> bufferedMiniTileStream, IObservable<PlusCode> location)
        {
            var output = bufferedMiniTileStream.WithLatestFrom(location, (tiles, code) => new { tiles, code }).Scan(new List<MiniTile>(), (list, l1) =>
            {

                list = TileGenerator.RegenerateArea(l1.code,list, l1.tiles, 40);

                return list;
            });
            return output;
            

        }




        IObservable<Tile> LargeTileUpdate(IObservable<DataSourceEvent> observable) => from e in observable
                                                                                      select JsonConvert.DeserializeObject<Tile>(e.Data);

        IObservable<MiniTile> AsMiniTiles(IObservable<Tile> observable) => from e in observable
                                                                           from v in e.MiniTiles
                                                                           select v;


        IObservable<MiniTileUpdate> MiniTileUpdate(IObservable<DataSourceEvent> observable) => from e in observable
                                                                                               select JsonConvert.DeserializeObject<MiniTileUpdate>(e.Data);

        IObservable<MiniTile> SmallUpdate(IObservable<MiniTileUpdate> observable) => from e in observable
                                                                                     select e.UpdatedTile;



    }
}

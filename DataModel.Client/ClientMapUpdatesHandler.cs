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
    public class ClientMapUpdatesHandler
    {
        readonly IEventBus eventBus;
        readonly List<MiniTile> miniTileBuffer = new List<MiniTile>();
        public ClientMapUpdatesHandler(IEventBus bus) => eventBus = bus;

        public IDisposable AttachToBus()
        {
            var receivedLarge = BufferMiniTiles(AsMiniTiles(LargeTileUpdate(eventBus.GetEventStream<DataSourceEvent>())));
            var receivedSmall = SmallUpdate(MiniTileUpdate(eventBus.GetEventStream<DataSourceEvent>()));
            var latestClient = LatestClientLocation(eventBus.GetEventStream<ClientGpsChangedEvent>());

            return RenderForUnity(receivedLarge, receivedSmall, latestClient).Subscribe(v => eventBus.Publish<LatestMapEvent>(new LatestMapEvent(v)));
        }





        IObservable<IList<MiniTile>> RenderForUnity(IObservable<IList<MiniTile>> observable1, IObservable<MiniTile> observable2, IObservable<PlusCode> location)
        {
            //var merge = observable1.Merge(observable2);
            return AsObservableBuffer(miniTileBuffer, observable1, location);
        }

        IObservable<IList<MiniTile>> BufferMiniTiles(IObservable<MiniTile> observable) => observable.Buffer(TimeSpan.FromSeconds(2));


        IObservable<IList<MiniTile>> AsObservableBuffer(List<MiniTile> buffer, IObservable<IList<MiniTile>> bufferedMiniTileStream, IObservable<PlusCode> code)
           => from c in code
              from s in bufferedMiniTileStream.Scan(buffer, (list, newValues) =>
              {
                  if (newValues == null)
                      return list;


                  //Replaces old values as new tiles arrive
                  //Removes tiles extremly far away
                  var oldValues = from v1 in list
                                  from v2 in newValues
                                  where (v1.PlusCode.Equals(v2.PlusCode) || (PlusCodeUtils.GetChebyshevDistance(c, v1.PlusCode) > 70))
                                  select v1;

                  var newList = list.Except(oldValues);

                  list.AddRange(newValues);
                  return list;
              })
              select s;

       

        //Breaking down large updates into minitiles stream
        IObservable<Tile> LargeTileUpdate(IObservable<DataSourceEvent> observable) => from e in observable
                                                                                      select JsonConvert.DeserializeObject<Tile>(e.Data);

        IObservable<MiniTile> AsMiniTiles(IObservable<Tile> observable) => from e in observable
                                                                           from v in e.MiniTiles
                                                                           select v;


        IObservable<MiniTileUpdate> MiniTileUpdate(IObservable<DataSourceEvent> observable) => from e in observable
                                                                                               select JsonConvert.DeserializeObject<MiniTileUpdate>(e.Data);

        IObservable<MiniTile> SmallUpdate(IObservable<MiniTileUpdate> observable) => from e in observable
                                                                                     select e.UpdatedTile;

        IObservable<PlusCode> LatestClientLocation(IObservable<ClientGpsChangedEvent> observable) => from e in observable
                                                                                                     select DataModelFunctions.GetPlusCode(e.ClientGPSHasChanged, 10);
        //TODO:FINISH

    }
}

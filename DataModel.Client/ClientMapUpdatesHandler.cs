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
        public ClientMapUpdatesHandler(IEventBus bus) => eventBus = bus;
        
        public IDisposable AttachToBus()
        {
            var receivedLarge = AsMiniTiles(LargeTileUpdate(eventBus.GetEventStream<DataSourceEvent>()));
            var receivedSmall = SmallUpdate(MiniTileUpdate(eventBus.GetEventStream<DataSourceEvent>()));
            var latestClient = LatestClientLocation(eventBus.GetEventStream<ClientGpsChangedEvent>());

            return RenderForUnity(receivedLarge, receivedSmall, latestClient).Subscribe(v => eventBus.Publish<LatestMapEvent>(new LatestMapEvent(v)));
        }

   

        //TODO: Fix this
        IObservable<IList<MiniTile>> RenderForUnity(IObservable<MiniTile> observable1, IObservable<MiniTile> observable2, IObservable<PlusCode> location)
        {
            observable1.Subscribe(v => Debug.WriteLine(v.ToString()));
            //observable2.Subscribe(v => Debug.WriteLine(v.ToString()));


            var merge = observable1.Merge(observable2);
            var fittingTiles = from e in merge
                               from v in location
                               where PlusCodeUtils.GetChebyshevDistance(v, e.PlusCode) < 20
                               select e;
            return fittingTiles.ToList();
        }


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
                                                                                                     select DataModelFunctions.GetPlusCode(e.NewGPS, 10);
        //TODO:FINISH


        public static void CollectToMiniTileList(IObservable<MiniTile> miniTile, IObservable<ClientGpsChangedEvent> observable, List<MiniTile> miniTileList)
        {
            PlusCode currentMiniTileCode = new PlusCode();
            observable.Subscribe(p => currentMiniTileCode = DataModelFunctions.GetPlusCode(p.NewGPS, 10));

            miniTile.Subscribe(v =>
            {
              
                //add minitile to list if its not already in there
                if (!miniTileList.Contains(v)){
                    miniTileList.Add(v);
                }

                //if there is a current miniTile code (on which the player is)
                if (currentMiniTileCode.Code != null)
                {
                    //remove all minitiles that are further away than 200 miniTiles from the list
                    foreach(MiniTile t in miniTileList)
                    if (PlusCodeUtils.GetChebyshevDistance(t.PlusCode, currentMiniTileCode) > 200)
                    {
                            miniTileList.Remove(t);
                    }
                }
            });
        }

        public static void CollectToTileList(IObservable<Tile> tile , List<Tile> tileList)
        {
            tile.Subscribe(v =>
            {


            });
        }
    }
}

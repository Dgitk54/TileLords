using DataModel.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Reactive.Linq;

namespace DataModel.Client
{
    public class MapAsRenderableForUnityHandler
    {

        readonly IEventBus eventBus;
        public MapAsRenderableForUnityHandler(IEventBus bus)
        {
            eventBus = bus;
        }
        public IDisposable AttachToBus()
        {
            var latestClientLocation = ClientFunctions.LatestClientLocation(eventBus.GetEventStream<UserGpsEvent>());
            var latestBuffer = eventBus.GetEventStream<ClientMapBufferChanged>();


            return latestClientLocation.CombineLatest(latestBuffer, (loc, buffer) => new { loc, buffer }).DistinctUntilChanged()
                                       .Subscribe(v => 
                                       {
                                           var neighbors = v.loc.Neighbors(10);
                                           var sortedList = new List<MiniTile>();
                                           int nullTiles = 0;
                                           neighbors.ForEach(c => 
                                           {
                                               var tile = c.GetMiniTile(v.buffer.TilesToRenderForUnity);

                                               if (tile != null) 
                                               {
                                                   sortedList.Add(c.GetMiniTile(v.buffer.TilesToRenderForUnity));
                                               } else 
                                               {
                                                   nullTiles++;
                                                   sortedList.Add(new MiniTile(c, MiniTileType.Unknown_Tile, null));
                                               }
                                                
                                               
                                           });
                                           sortedList = LocationCodeTileUtility.SortList(sortedList);
                                           var map = new MapAsRenderAbleChanged() { Location = v.loc, Map = sortedList, NullTiles = nullTiles };
                                           eventBus.Publish(map);
                                       });
        }

        
    }
}

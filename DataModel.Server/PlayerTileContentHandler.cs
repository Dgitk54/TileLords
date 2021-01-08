using DataModel.Common;
using Google.OpenLocationCode;
using LiteDB;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;

namespace DataModel.Server
{
    /// <summary>
    /// Serverwide handler responsible for delivering content updates to the clients.
    /// </summary>
    public class PlayerTileContentHandler
    {

        readonly IEventBus eventBus;
        public PlayerTileContentHandler(IEventBus serverBus)
        {
            eventBus = serverBus;
        }
        public IDisposable AttachToBus()
        {

            var allOnlinePlayers = eventBus.GetEventStream<PlayersOnlineEvent>();

            var allMapEvents = eventBus.GetEventStream<ServerMapEvent>();


            var mergedUpdate = allMapEvents.WithLatestFrom(allOnlinePlayers, (map, players) => new { map, players });

            return mergedUpdate.Subscribe(v =>
            {
                
                v.players.PlayersOnline.ToList().ForEach(v2 =>
                {
                    var bus = v2.ClientBus;

                    var rawContent = new Dictionary<PlusCode, List<ITileContent>>();

                    v.map.MiniTiles.ToList().ForEach(v3 =>
                    {
                        var tile = v3;
                        rawContent.Add(tile.MiniTileId, new List<ITileContent>(tile.Content));
                    });
                    var content = new ServerTileContentEvent() { VisibleContent = rawContent.ToList() };
                    bus.Publish(content);

                });
            });

       
        }

        
    }
}

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


            var updateOccured = from playerEvent in allOnlinePlayers
                                from player in playerEvent.PlayersOnline
                                from playerLocation in player.PlayerObservableLocationStream
                                from mapEvent in allMapEvents
                                from changedMiniTile in mapEvent.MiniTiles
                                where playerLocation.GetChebyshevDistance(changedMiniTile.MiniTileId) < 50
                                select (player, changedMiniTile);

            return updateOccured.Subscribe(v =>
            {
                var bus = v.player.ClientBus;
                var tile = v.changedMiniTile;

                var rawContent = new Dictionary<PlusCode, List<ITileContent>>()
                {
                    {tile.MiniTileId, new List<ITileContent>(tile.Content)  }
                };


                var content = new ServerTileContentEvent() { VisibleContent = rawContent.ToList() };
                bus.Publish(content);
            });


        }
    }
}

using DataModel.Common.GameModel;
using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace DataModel.Server.Services
{
    /// <summary>
    /// Service dealing with inventory interaction of players.
    /// </summary>
    public class InventoryService
    {
        public IObservable<Dictionary<InventoryType, int>> RequestPlayerInventory(byte[] playerId)
        {
            return Observable.Create<Dictionary<InventoryType, int>>(v =>
             {
                 var result = MongoDBFunctions.RequestInventory(playerId, playerId).Result;
                 if (result == null)
                 {
                     v.OnError(new Exception("No inventory for player found!"));
                     return Disposable.Empty;
                 }

                 v.OnNext(result);
                 v.OnCompleted();
                 return Disposable.Empty;
             });
        }
        public IObservable<(Dictionary<InventoryType, int>, byte[])> RequestContainerInventory(byte[] playerId, byte[] containerId)
        {
            return Observable.Create<(Dictionary<InventoryType, int>, byte[])>(v =>
             {
                 var inventory = MongoDBFunctions.RequestInventory(playerId, containerId);
                 if (inventory == null)
                 {
                     v.OnError(new Exception("Could not fetch inventory for id"));
                     return Disposable.Empty;
                 }
                 v.OnNext((inventory.Result, containerId));
                 v.OnCompleted();
                 return Disposable.Empty;
             });

        }
        public IObservable<(bool, byte[])> MapContentPickUp(byte[] requestId, byte[] mapContentTarget)
        {
            return Observable.Create<(bool, byte[])>(v =>
            {
                var result = MongoDBFunctions.RemoveContentAndAddToPlayer(requestId, mapContentTarget).Result;
                v.OnNext((result, mapContentTarget));
                v.OnCompleted();
                return Disposable.Empty;
            });
        }

    }
}

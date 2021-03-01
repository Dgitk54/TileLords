using DataModel.Common.Messages;
using DataModel.Server.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;

namespace DataModel.Server.Services
{
    /// <summary>
    /// Service dealing with inventory interaction of players.
    /// </summary>
    public class InventoryService
    {
        public IObservable<Dictionary<ResourceType, int>> RequestPlayerInventory(byte[] playerId)
        {
            return Observable.Create<Dictionary<ResourceType, int>>(v =>
             {
                 var result = DataBaseFunctions.RequestInventory(playerId, playerId);
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
        public IObservable<(Dictionary<ResourceType, int>, byte[])> RequestContainerInventory(byte[] playerId, byte[] containerId)
        {
            return Observable.Create<(Dictionary<ResourceType, int>, byte[])>(v =>
             {
                 var inventory = DataBaseFunctions.RequestInventory(playerId, containerId);
                 if (inventory == null)
                 {
                     v.OnError(new Exception("Could not fetch inventory for id"));
                     return Disposable.Empty;
                 }
                 v.OnNext((inventory, containerId));
                 v.OnCompleted();
                 return Disposable.Empty;
             });

        }
        //TODO: BUG: synchronize properly
        public IObservable<(bool, byte[])> MapContentPickUp(byte[] requestId, byte[] mapContentTarget)
        {
            return Observable.Create<(bool, byte[])>(v =>
            {
                var result = DataBaseFunctions.RemoveContentAndAddToPlayer(requestId, mapContentTarget);
                v.OnNext((result, mapContentTarget));
                v.OnCompleted();
                return Disposable.Empty;
            });
        }

    }
}

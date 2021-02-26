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


        readonly Func<byte[], MapContent> dataBaseMapPickupFunction;
        readonly Func<byte[], MapContent, bool> dataBaseAddPlayerInventoryFunction;
        readonly Func<byte[], byte[], bool> hasStorageSpaceForTarget;
        readonly Func<byte[], Inventory> dataBasePlayerInventoryLookup;
        public InventoryService(Func<byte[], MapContent> dataBaseMapPickupFunction, 
                                Func<byte[], MapContent, bool> dataBaseAddPlayerInventoryFunction, 
                                Func<byte[], byte[], bool> hasStorageSpaceForTarget, 
                                Func<byte[], Inventory> dataBasePlayerInventoryLookup)
        {
            this.dataBaseMapPickupFunction = dataBaseMapPickupFunction;
            this.dataBaseAddPlayerInventoryFunction = dataBaseAddPlayerInventoryFunction;
            this.hasStorageSpaceForTarget = hasStorageSpaceForTarget;
            this.dataBasePlayerInventoryLookup = dataBasePlayerInventoryLookup;
        }

        public IObservable<Inventory> RequestPlayerInventory(byte[] playerId)
        {
            return Observable.Create<Inventory>(v =>
            {
                var result = dataBasePlayerInventoryLookup(playerId);
                if (result == null)
                {
                    v.OnError(new Exception("No inventory for player found!"));
                    return Disposable.Empty;
                }
                Debug.Assert(result.ContainerId.SequenceEqual(playerId));
                Debug.Assert(result.OwnerId.SequenceEqual(playerId));
                v.OnNext(result);
                v.OnCompleted();
                return Disposable.Empty;
            });

        }

        //TODO: BUG: synchronize properly
        public IObservable<bool> MapContentPickUp(byte[] requestId, byte[] mapContentTarget)
        {
            return Observable.Create<bool>(v =>
            {
                var hasSpace = hasStorageSpaceForTarget(requestId, mapContentTarget);
                if (!hasSpace)
                {
                    v.OnNext(false);
                    v.OnCompleted();
                    return Disposable.Empty;
                }

                var result = dataBaseMapPickupFunction(mapContentTarget);
                if (result == null)
                {
                    v.OnNext(false);
                    v.OnCompleted();
                    return Disposable.Empty;
                }
                var inventoryInsertResult = dataBaseAddPlayerInventoryFunction(requestId, result);
                if (!inventoryInsertResult)
                {
                    v.OnNext(false);
                    v.OnCompleted();
                    return Disposable.Empty;
                }

                v.OnNext(true);
                v.OnCompleted();
                return Disposable.Empty;
            });
        }

    }
}

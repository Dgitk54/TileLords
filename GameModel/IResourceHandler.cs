using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel
{
    interface IResourceHandler
    {

        IObservable<Resource> ResourceSpawned(IObservable<Resource> resource, IObservable<int> amount, IObservable<Tile> tile);
        IObservable<Resource> ResourceDespawned(IObservable<Resource> resource, IObservable<int> amount, IObservable<Tile> tile);
        IObservable<Resource> ResourceUsed(IObservable<Resource> resource, IObservable<int> amount);
        IObservable<Resource> ResourceGained(IObservable<Resource> resource, IObservable<int> amount);


    }
}

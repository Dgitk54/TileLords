using DataModel.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Concurrency;
using System.Diagnostics;
using System.Threading.Tasks;
using LiteDB;
using DataModel.Common.Messages;
using System.Linq;

namespace DataModel.Server.Services
{

    /// <summary>
    /// Class responsible for spawning content on the map
    /// </summary>
    public class ResourceSpawnService
    {
        const int RESOURCEALIVEINSEC = 60;
        readonly MapContentService service;
        readonly List<IDisposable> disposables = new List<IDisposable>();
        readonly Action<MapContent, string> userContentStorage;
        readonly List<Func<List<MapContent>, bool>> spawnCheckFunctions;
        readonly Subject<(Resource, string)> storedMapResources = new Subject<(Resource, string)>();
        readonly ISubject<(Resource, string)> resourceSpawnRequests;

        public ResourceSpawnService(MapContentService service, Action<MapContent, string> userContentStorage, List<Func<List<MapContent>, bool>> spawnCheckFunctions)
        {
            this.service = service;
            this.userContentStorage = userContentStorage;
            this.spawnCheckFunctions = spawnCheckFunctions;

            resourceSpawnRequests = Subject.Synchronize(storedMapResources);

            //var disposable = resourceSpawnRequests.Select(v => AddOnMapWithDespose(Observable.Return(v)))
            //                                      .Subscribe(v=> disposables.Add(v));

            var disposable = resourceSpawnRequests.Subscribe(v => userContentStorage(v.Item1.AsMapContent(), v.Item2));
            disposables.Add(disposable);
        }

        public void ShutDownService()
        {
            disposables.ForEach(v => v.Dispose());
        }
        
       
        //TODO: ERRORPRONE! Propagate location downstream via touples
        public IDisposable AddMovableRessourceSpawnArea(byte[] moveableOwnerId, IObservable<PlusCode> location)
        {

            return location.Throttle(TimeSpan.FromSeconds(20)) //Throttle location
                           .Select(v => SpawnConditionMet(v)) //Check if spawn condition applies
                           .Switch()
                           .Where(v => v)               //Stream of where spawn conditions are true 
                           .Select(v => GetRandomResource(moveableOwnerId))  //Grab a spawnable ressource for the id
                           .Switch()
                           .WithLatestFrom(location, (res, loc) => new { res, loc }) //merge with latest location
                           .Subscribe(v =>
                           {
                               resourceSpawnRequests.OnNext((v.res, v.loc.Code));
                           });
        }

        



        IDisposable AddOnMapWithDespose(IObservable<(Resource, string)> resourceWithLocation)
        {
            return resourceWithLocation.Select(v => service.AddMapContent(v.Item1.AsMapContent(), GetNearbyRandomSpawn(v.Item1, v.Item2)))
                                       .Delay(TimeSpan.FromSeconds(RESOURCEALIVEINSEC))
                                       .Subscribe(v => v.Dispose());

        }

        


        IObservable<PlusCode> GetNearbyRandomSpawn(Resource resource, string vicinity)
        {
            return Observable.Create<PlusCode>(v =>
            {
                var list = LocationCodeTileUtility.GetTileSection(vicinity, 10, 10);
                var randomSpot = new Random().Next(list.Count);
                v.OnNext(new PlusCode(list[randomSpot], 10));
                v.OnCompleted();
                return Disposable.Empty;
            });
        }


        IObservable<Resource> GetRandomResource(byte[] requestId)
        {
            return Observable.Create<Resource>(v =>
            {
                v.OnNext(ServerFunctions.GetRandomNonQuestResource());
                v.OnCompleted();
                return Disposable.Empty;
            });
            
        }

        IObservable<Resource> GetRandomQuestResource(byte[] requestId)
        {
            throw new NotImplementedException();
        }

        IObservable<bool> SpawnConditionMet(PlusCode code)
        {
            return service.GetListMapUpdate(code.Code).Select(v =>
            {
                return spawnCheckFunctions.All(v2 =>
                {
                    return v2.Invoke(v);
                });
            });
        }
    }
}

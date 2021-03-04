﻿using DataModel.Common;
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
using DataModel.Server.Model;

namespace DataModel.Server.Services
{

    /// <summary>
    /// Class responsible for spawning/despawning content on the map
    /// </summary>
    public class ResourceSpawnService
    {
        const int RESOURCEALIVEINSEC = 60;
        const int RESOURCESPAWNCHECKTHROTTLEINSECONDS = 15;
        readonly MapContentService service;
        readonly List<IDisposable> disposables = new List<IDisposable>();
        readonly List<Func<List<MapContent>, bool>> spawnCheckFunctions;
        readonly Subject<(Resource, string)> storedMapResources = new Subject<(Resource, string)>();
        readonly ISubject<(Resource, string)> resourceSpawnRequests;

        public ResourceSpawnService(MapContentService service, Action<MapContent, string> userContentStorage, List<Func<List<MapContent>, bool>> spawnCheckFunctions)
        {
            this.service = service;
            this.spawnCheckFunctions = spawnCheckFunctions;

            resourceSpawnRequests = Subject.Synchronize(storedMapResources);

            //Handling spawn requests
            var disposable = resourceSpawnRequests.Do(v => Console.WriteLine("Spawning content!"))
                                                  .Subscribe(v => userContentStorage(v.Item1.AsMapContent(), v.Item2));
            
            //Handling delete requests
            var deleteRequests = resourceSpawnRequests.Delay(TimeSpan.FromSeconds(RESOURCEALIVEINSEC))
                                                      .Do(v => Console.WriteLine("Despawning content!"))
                                                      .Subscribe(v => userContentStorage(v.Item1.AsMapContent(), null));

            disposables.Add(disposable);
            disposables.Add(deleteRequests);
        }

        public void ShutDownService()
        {
            disposables.ForEach(v => v.Dispose());
        }

        //TODO: minor bugprone: Propagate location downstream via touples
        public IDisposable AddMovableResourceSpawnArea(byte[] moveableOwnerId, IObservable<PlusCode> location)
        {
            return Observable.Interval(TimeSpan.FromSeconds(RESOURCESPAWNCHECKTHROTTLEINSECONDS))
                             .WithLatestFrom(location, (_, loc) => new { _, loc})
                             .Select(v=> v.loc)
                             .SpawnConditionMet(service,spawnCheckFunctions)
                             .Where(v=> v)
                             .Select(v=> GetRandomResource(moveableOwnerId))
                             .Switch()
                             .WithLatestFrom(location, (res, loc) => new { res, loc })
                             .Subscribe(v =>
                             {
                                 var randomNearbyLocation = DataModelFunctions.GetNearbyRandomSpawn(v.loc.Code, 10).Code;
                                 resourceSpawnRequests.OnNext((v.res, randomNearbyLocation));
                             });
        }

        public IDisposable AddMoveableQuestResourceSpawnArea(byte[] moveableOwnerId, IObservable<PlusCode> location)
        {
            return null;
            //TODO: FINISH
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
    }
}

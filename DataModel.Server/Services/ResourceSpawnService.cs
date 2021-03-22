using DataModel.Common;
using DataModel.Server.Model;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace DataModel.Server.Services
{

    /// <summary>
    /// Class responsible for spawning/despawning content on the map
    /// </summary>
    public class ResourceSpawnService
    {
        const int RESOURCEALIVEINSEC = 420; //TODO: Find a structure to handle quest resource decay differently
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
            var disposable = resourceSpawnRequests.Subscribe(v => userContentStorage(v.Item1.AsMapContent(), v.Item2));

            //Handling delete requests
            var deleteRequests = resourceSpawnRequests.Delay(TimeSpan.FromSeconds(RESOURCEALIVEINSEC)).Subscribe(v => userContentStorage(v.Item1.AsMapContent(), null));

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
                             .WithLatestFrom(location, (_, loc) => new { _, loc })
                             .Select(v => v.loc)
                             .SpawnConditionMet(service, spawnCheckFunctions)
                             .Where(v => v)
                             .Select(v => GetRandomResource(moveableOwnerId))
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
            return Observable.Interval(TimeSpan.FromSeconds(RESOURCESPAWNCHECKTHROTTLEINSECONDS)) // use timer => in case player doesnt move
                                .WithLatestFrom(location, (_, loc) => new { _, loc })
                                .Select(v => v.loc) // discard timer, reduce only to location
                                .Select(v => (LiteDBDatabaseFunctions.GetQuestsForUser(moveableOwnerId), v))
                                .Where(v => v.Item1 != null) //User has Quests
                                .Select(v => ServerFunctions.LocationIsInsideQuestSpawnableArea(v.v, v.Item1)) //Enumerable of quests zones the player is inside
                                .Where(v => v.Any()) //Only when player is inside questzone
                                .Select(v => v.Where(e => ServerFunctions.ShouldResourceSpawn(e, RESOURCESPAWNCHECKTHROTTLEINSECONDS))) // Calculate Questitemspawnchance
                                .Where(v => v.Any()) //Only when item should spawn
                                .Subscribe(v => //Handle SpawnRequests
                                {
                                    v.ToList().ForEach(e =>
                                    {
                                        //random nearby location
                                        var randomLocationInSpawnArea = DataModelFunctions.GetNearbyRandomSpawn(e.Quest.QuestTargetLocation, e.Quest.AreaRadiusFromLocation);
                                        //get proper resource to spawn for quest
                                        var resource = e.ExtractQuestResource(randomLocationInSpawnArea.Code);
                                        //send spawn request
                                        resourceSpawnRequests.OnNext((resource, randomLocationInSpawnArea.Code)); // Push into internal observable stream
                                    });
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
    }
}

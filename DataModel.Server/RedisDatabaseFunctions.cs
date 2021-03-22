using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DataModel.Common;
using DataModel.Server.Model;
using MessagePack;
using StackExchange.Redis;

namespace DataModel.Server
{
    public static class RedisDatabaseFunctions
    {
        public static ConnectionMultiplexer redis { get; }
        static RedisDatabaseFunctions()
        {
            //Docker run -p INC:OUT --name -d redis
            redis = ConnectionMultiplexer.Connect("192.168.1.184,allowAdmin=true");
            
            EndPoint endPoint = redis.GetEndPoints().First();
            var server = redis.GetServer(endPoint);
            server.FlushAllDatabases();
        }

        static RedisKey[] latestKeys;
        static DateTime latestRequest;
        static Mutex redisKeyRequestLock = new Mutex();

        public static void StoreValue(string key, string value)
        {
            var db = redis.GetDatabase();
            db.StringSet(key, value);
            var debug = db.StringGet(key);
            Console.WriteLine(debug);
        }

        public static void UpsertOrDeleteContent(MapContent content, string location)
        {
            var db = redis.GetDatabase();
            var idAsString = Convert.ToBase64String(content.Id);
            if (location != null)
            {
                var contentWithLocation = new MapContent() { CanBeLootedByPlayer = content.CanBeLootedByPlayer, Id = content.Id, Location = location, Name = content.Name, ResourceType = content.ResourceType, Type = content.Type };
                var contentAsByte = MessagePackSerializer.Serialize(contentWithLocation);
                var redisMapValue = DatabaseMapStorage.ToDatabaseString(location, contentAsByte);
                db.StringSet(idAsString, redisMapValue);
            }
            else
            {
                db.KeyDelete(idAsString);
            }
        }
        
        //TODO: Partitioning per zone
        public static List<MapContent> RequestVisibleContent(string location)
        {
            EndPoint endPoint = redis.GetEndPoints().First();
            RedisKey[] keys = KeyPoll();
            var db = redis.GetDatabase();
            var nearbyCodes = LocationCodeTileUtility.GetTileSection(location, ServerFunctions.CLIENTVISIBILITY, ServerFunctions.CLIENTLOCATIONPRECISION);
            var set = new HashSet<string>(nearbyCodes);

            var batch = db.CreateBatch();

            var taskList = new List<Task<RedisValue>>();
            keys.ToList().ForEach(v => taskList.Add(batch.StringGetAsync(v)));
            batch.Execute();
            Task.WhenAll(taskList.ToArray());
            ConcurrentBag<MapContent> result = new ConcurrentBag<MapContent>();
            taskList.AsParallel().ForAll(v =>
            {

                var storedObject = v.Result;
                if (storedObject.HasValue)
                {
                    var redisValue = storedObject.ToString();
                    var content = DatabaseMapStorage.FromRedisValue(redisValue);

                    if (set.Contains(content.Location))
                    {
                        var mapContentObject = MessagePackSerializer.Deserialize<MapContent>(content.MapContentObject);
                        result.Add(mapContentObject);
                    }
                }
            });
            return result.ToList();
        }

       
        public  static  RedisKey[] KeyPoll()
        {
            if (latestKeys == null) 
            {
                redisKeyRequestLock.WaitOne();
                EndPoint endPoint = redis.GetEndPoints().First();
                latestRequest = DateTime.Now;
                latestKeys = redis.GetServer(endPoint).Keys(pattern: "*").ToArray();
                redisKeyRequestLock.ReleaseMutex();
            };
            if((DateTime.Now - latestRequest).TotalSeconds > 3)
            {
                redisKeyRequestLock.WaitOne();
                if((DateTime.Now - latestRequest).TotalSeconds < 3)
                {
                    redisKeyRequestLock.ReleaseMutex();
                    return latestKeys;
                }
                latestRequest = DateTime.Now;
                var endPoint = redis.GetEndPoints().First();
                var updateTask = Task.Run(() => redis.GetServer(endPoint).Keys(pattern: "*").ToArray());
                redisKeyRequestLock.ReleaseMutex();
                updateTask.Wait();
                latestKeys = updateTask.Result;
            }
            return latestKeys;
        }
            }
}

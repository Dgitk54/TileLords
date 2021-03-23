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
        public static ConnectionMultiplexer Redis { get; }
        static RedisDatabaseFunctions()
        {
            //Docker run -p INC:OUT --name -d redis
            Redis = ConnectionMultiplexer.Connect("192.168.1.184,allowAdmin=true");

            EndPoint endPoint = Redis.GetEndPoints().First();
            var server = Redis.GetServer(endPoint);
            server.FlushAllDatabases();
        }



        public static List<MapContent> RequestVisibleContent(double lat, double lon)
        {
            var db = Redis.GetDatabase();
            var keys = db.GeoRadius("mapcontent", lon, lat, 300);
            var batch = db.CreateBatch();

            var taskList = new List<Task<RedisValue>>();
            keys.ToList().ForEach(v => taskList.Add(batch.StringGetAsync(v.Member.ToString())));
            batch.Execute();
            Task.WhenAll(taskList.ToArray());
            ConcurrentBag<MapContent> result = new ConcurrentBag<MapContent>();
            taskList.AsParallel().ForAll(v =>
            {
                var storedObject = v.Result;
                if (storedObject.HasValue)
                {
                    var redisValue = storedObject.ToString();
                    var mapContentRaw = Convert.FromBase64String(redisValue);
                    var mapContentObject = MessagePackSerializer.Deserialize<MapContent>(mapContentRaw);
                    result.Add(mapContentObject);
                }
            });
            return result.ToList();
        }


        public static bool RemoveContent(MapContent content)
        {
            var db = Redis.GetDatabase();
            var idAsString = Convert.ToBase64String(content.Id);
            var batch = db.CreateBatch();
            var removeTask = batch.GeoRemoveAsync("mapcontent", idAsString);
            var keydeletetask = batch.KeyDeleteAsync(idAsString);
            batch.Execute();
            Task.WhenAll(removeTask, keydeletetask);
            return removeTask.Result && keydeletetask.Result;
        }

        public static void UpsertContent(MapContent content, double lat, double lon)
        {
            var db = Redis.GetDatabase();
            var idAsString = Convert.ToBase64String(content.Id);
            var batch = db.CreateBatch();

            var add = batch.GeoAddAsync("mapcontent", lon, lat, idAsString);
            var plusCode = new GPS(lat, lon).GetPlusCode(10);
            var contentWithLocation = new MapContent() { CanBeLootedByPlayer = content.CanBeLootedByPlayer, Id = content.Id, Location = plusCode.Code, Name = content.Name, ResourceType = content.ResourceType, Type = content.Type };
            
            var contentAsByte = MessagePackSerializer.Serialize(contentWithLocation);
            var contentAsString = Convert.ToBase64String(contentAsByte);
           
            var stringSet = batch.StringSetAsync(idAsString, contentAsString);

            batch.Execute();
            Task.WhenAll(add, stringSet);
        }


        
       

    }
}

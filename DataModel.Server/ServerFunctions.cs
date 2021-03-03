using DataModel.Common;
using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Joins;
using Google.OpenLocationCode;
using DotNetty.Transport.Channels;
using System.Diagnostics;
using DotNetty.Buffers;
using LiteDB;
using Newtonsoft.Json.Serialization;
using System.Security.Cryptography;
using DataModel.Common.Messages;
using DataModel.Server.Services;
using DataModel.Server.Model;
using DataModel.Common.GameModel;

namespace DataModel.Server
{
    /// <summary>
    /// Class with functions shared between multiple handlers.
    /// </summary>
    public static class ServerFunctions
    {
        public readonly static int CLIENTVISIBILITY = 10;
        public readonly static int CLIENTLOCATIONPRECISION = 10;
        

        public static bool Only5ResourcesInArea(List<MapContent> content)
        {
            return content.Where(v => v.Type == ContentType.RESOURCE).Count() < 5;
        }
        public static Dictionary<ResourceType, int> ToResourceDictionary(this MapContent content)
        {
            return new Dictionary<ResourceType, int>() { { content.ResourceType, 1 } };
        }
        public static Resource GetRandomNonQuestResource()
        {
            Array values = Enum.GetValues(typeof(ResourceType));
            Random random = new Random();
            ResourceType randomType = (ResourceType)values.GetValue(random.Next(1, values.Length));
            var id = ObjectId.NewObjectId().ToByteArray();
            return new Resource() { Id = id, Location = null, Name = randomType.ToString(), ResourceType = randomType, Type = ContentType.RESOURCE };
        }

        public static bool PlayerCanLootObject(List<QuestContainer> currentPlayerQuests, MapContent contentToCheck)
        {
            if (!contentToCheck.CanBeLootedByPlayer)
                return false;
            if (contentToCheck.Type == ContentType.RESOURCE)
                return true;
            
            if(currentPlayerQuests != null)
                return currentPlayerQuests.Select(v => v.Quest).Any(v => v.QuestHasItemAsTarget(contentToCheck));
            return false;
        }
        public static bool QuestHasItemAsTarget(this Quest quest, MapContent contentToCheck) 
        {
            if (quest.QuestLevel != contentToCheck.Type)
                return false;
            if (quest.TypeToPickUp != contentToCheck.ResourceType)
                return false;
            return true;
        }

        public static IObservable<bool> SpawnConditionMet(this IObservable<PlusCode> code, MapContentService service, List<Func<List<MapContent>, bool>> spawnCheckFunctions)
        {
            return code.Select(v => service.GetListMapUpdate(v.Code)).Switch().Select(v =>
            {
                return spawnCheckFunctions.All(v2 =>
                {
                    return v2.Invoke(v);
                });
            });
        }

        public static MapContent AsMapContent(this IUser user)
        {
            return new MapContent() { Id = user.UserId, Name = user.UserName, ResourceType = ResourceType.NONE, Type = ContentType.PLAYER, Location = null, MapContentId = null, CanBeLootedByPlayer = false };
        }
        public static MapContent AsMapContent(this Resource resource)
        => new MapContent() { Id = resource.Id, Location = resource.Location, Name = resource.Name, ResourceType = resource.ResourceType, Type = resource.Type, CanBeLootedByPlayer = true };
        public static ContentMessage AsMessage(this MapContent content)
        => new ContentMessage() { Id = content.Id, Location = content.Location, Name = content.Name, ResourceType = content.ResourceType, Type = content.Type };
        public static byte[] Hash(string value, byte[] salt) => Hash(Encoding.UTF8.GetBytes(value), salt);

        public static byte[] Hash(byte[] value, byte[] salt)
        {
            var pbkdf2 = new Rfc2898DeriveBytes(value, salt, 10000);
            return pbkdf2.GetBytes(20);
        }

        public static bool PasswordMatches(byte[] password, byte[] originalPassword, byte[] originalSalt)
        {
            var pbkdf2 = new Rfc2898DeriveBytes(password, originalSalt, 10000);
            var result = pbkdf2.GetBytes(20);
            return result.SequenceEqual(originalPassword);
        }

    }
}

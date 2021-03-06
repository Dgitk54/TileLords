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

        public readonly static int CLIENT_MAXQUESTS = 5;

        //QUESTLEVEL 1:
        public readonly static int QUESTLEVEL1_DECAYINDAYS = 14;
        public readonly static int QUESTLEVEL1_MINDISTANCEFROMREQUESTSPAWN_INMINITILES = 40;
        public readonly static int QUESTLEVEL1_MAXDISTANCEFROMREQUESTSPAWN_INMINITILES = 60;
        public readonly static int QUESTLEVEL1_SPAWNAREA_INMINITILES = 10;
        public readonly static int QUESTLEVEL1_REQUIREDAMOUNT_MAX = 15;

        //Magic numbers for spawnchance calculation:
        public readonly static double ONE_PER_MINUTEAVARAGE = 1.67;
        public readonly static double TWO_PER_MINUTEAVARAGE = 3.34;

        //QUESTCONTAINER LEVEL 1:
        public readonly static double CONTAINER_1_ITEMSPAWNCHANCE_PERSECOND = TWO_PER_MINUTEAVARAGE;
        public readonly static int CONTAINER_1_MAXITEMS_PERAREA = 10;
        public readonly static int CONTAINER_1_ITEMALIVE_INSECONDS = 420;


        public static bool Only5ResourcesInArea(List<MapContent> content)
        {
            return content.Where(v => v.Type == ContentType.RESOURCE).Count() < 5;
        }



        public static Dictionary<ItemType, int> ToResourceDictionary(this MapContent content)
        {
            return new Dictionary<ItemType, int>() { { new ItemType() { ContentType= content.Type, ResourceType = content.ResourceType }, 1 } };
        }

        public static Resource GetRandomNonQuestResource()
        {
            Array values = Enum.GetValues(typeof(ResourceType));
            Random random = new Random();
            ResourceType randomType = (ResourceType)values.GetValue(random.Next(1, values.Length));
            var id = ObjectId.NewObjectId().ToByteArray();
            return new Resource() { Id = id, Location = null, Name = randomType.ToString(), ResourceType = randomType, Type = ContentType.RESOURCE };
        }
        public static Resource ExtractQuestResource(this QuestContainer quest, string location)
        {
            return new Resource() { Id = ObjectId.NewObjectId().ToByteArray(), Location = location, Name = quest.Quest.TypeToPickUp.ToString(), ResourceType = quest.Quest.TypeToPickUp, Type = quest.Quest.QuestLevel };
        }

        public static bool PlayerCanLootObject(List<QuestContainer> currentPlayerQuests, MapContent contentToCheck)
        {
            if (!contentToCheck.CanBeLootedByPlayer)
                return false;
            if (contentToCheck.Type == ContentType.RESOURCE)
                return true;

            if (currentPlayerQuests != null)
                return currentPlayerQuests.Select(v => v.Quest).Any(v => v.QuestHasItemAsTarget(contentToCheck));
            return false;
        }
        
        public static bool ShouldResourceSpawn(QuestContainer container, int spawnCheckDelayInSeconds)
        {
            var perSpawnCheck = container.QuestItemSpawnChancePerSecond * spawnCheckDelayInSeconds;
            var random = new Random();
            var roll = random.Next(0, 101);
            return roll < perSpawnCheck;
        }
        
        
        public static IEnumerable<QuestContainer> LocationIsInsideQuestSpawnableArea(PlusCode locationToTest, List<QuestContainer> quests)
        {
            return quests.Where(v =>
            {
                var targetLocation = v.Quest.QuestTargetLocation;
                var radius = v.Quest.AreaRadiusFromLocation;
                var section = LocationCodeTileUtility.GetTileSection(targetLocation, radius, 10);

                return section.Contains(locationToTest.Code);
            });
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

        public static bool ResourceCanSpawn(TileType location, ResourceType type)
        {
            var tileSpawnDictionary = WorldSpawnWeights.TileTypeToResourceTypeWeights[location];
            return tileSpawnDictionary.Keys.Contains(type);
        }

        public static ResourceType GetRandomResourceTypeForArea(string areaLocationIn10)
        {
            var tiletype = WorldGenerator.GetTileTypeForArea(new PlusCode(areaLocationIn10, 10));
            var dictionaryForTile = WorldSpawnWeights.TileTypeToResourceTypeWeights[tiletype];
            var randomDictionaryValue = new Random().Next(dictionaryForTile.Keys.Count);
            var randomType = dictionaryForTile.Keys.ToList()[randomDictionaryValue];
            return randomType;
        }
        public static QuestContainer WrapLevel1Quest(this Quest quest, byte[] owner)
        {
            return new QuestContainer()
            {
                OwnerId = owner,
                Quest = quest,
                QuestCreatedOn = DateTime.Now,
                QuestHasExpired = false,
                QuestItemAliveTimeInSeconds = CONTAINER_1_ITEMALIVE_INSECONDS,
                QuestItemsMaxAliveInQuestArea = CONTAINER_1_MAXITEMS_PERAREA,
                QuestItemSpawnChancePerSecond = CONTAINER_1_ITEMSPAWNCHANCE_PERSECOND
            };
        }
        public static Quest GetLevel1QuestForUser(PlusCode questRequestStart)
        {
            var questTargetLocation = DataModelFunctions.GetNearbyLocationWithinMinMaxBounds
                (
                questRequestStart.Code,
                QUESTLEVEL1_MAXDISTANCEFROMREQUESTSPAWN_INMINITILES,
                QUESTLEVEL1_MINDISTANCEFROMREQUESTSPAWN_INMINITILES
                ).Code;
            var resourceTypeForQuest = GetRandomResourceTypeForArea(questTargetLocation);
            var quest = new Quest()
            {
                AreaRadiusFromLocation = 5,
                ExpiringDate = DateTime.Now.AddDays(QUESTLEVEL1_DECAYINDAYS),
                QuestLevel = ContentType.QUESTLEVEL1,
                QuestOriginId = null,
                QuestTargetLocation = questTargetLocation,
                QuestTurninLocation = null,
                RequiredAmountForCompletion = QUESTLEVEL1_REQUIREDAMOUNT_MAX,
                TypeToPickUp = resourceTypeForQuest,
                QuestId = ObjectId.NewObjectId().ToByteArray()
            };
            return quest;
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

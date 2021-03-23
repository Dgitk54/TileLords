using DataModel.Common;
using DataModel.Common.GameModel;
using DataModel.Common.Messages;
using DataModel.Server.Model;
using DataModel.Server.Services;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Security.Cryptography;
using System.Text;

namespace DataModel.Server
{
    /// <summary>
    /// Class with functions shared between multiple handlers.
    /// </summary>
    public static class ServerFunctions
    {
        public static readonly int CLIENTVISIBILITY = 10;
        public static readonly int CLIENTLOCATIONPRECISION = 10;

        public static readonly int CLIENT_MAXQUESTS = 5;

        //QUESTLEVEL 1:
        public static readonly int QUESTLEVEL1_DECAYINDAYS = 14;
        public static readonly int QUESTLEVEL1_MINDISTANCEFROMREQUESTSPAWN_INMINITILES = 40;
        public static readonly int QUESTLEVEL1_MAXDISTANCEFROMREQUESTSPAWN_INMINITILES = 60;
        public static readonly int QUESTLEVEL1_SPAWNAREA_INMINITILES = 10;
        public static readonly int QUESTLEVEL1_REQUIREDAMOUNT_MAX = 2;

        //Magic numbers for spawnchance calculation:
        public static readonly double ONE_PER_MINUTEAVARAGE = 1.67;
        public static readonly double TWO_PER_MINUTEAVARAGE = 3.34;

        //QUESTCONTAINER LEVEL 1:
        public static readonly double CONTAINER_1_ITEMSPAWNCHANCE_PERSECOND = TWO_PER_MINUTEAVARAGE;
        public static readonly int CONTAINER_1_MAXITEMS_PERAREA = 10;
        public static readonly int CONTAINER_1_ITEMALIVE_INSECONDS = 420;


        public static bool Only5ResourcesInArea(List<MapContent> content)
        {
            return content.Where(v => v.Type == ContentType.RESOURCE).Count() < 5;
        }

        public static InventoryType GetQuestItemDictionaryKey(this Quest quest)
            => new InventoryType() { ContentType = quest.QuestLevel, ResourceType = quest.TypeToPickUp };

        public static Dictionary<InventoryType, int> ToResourceDictionary(this List<QuestReward> reward)
        {
           return reward.GroupBy(x => new InventoryType() { ContentType = x.ContentType, ResourceType = x.ResourceType }, x => x.Amount).ToDictionary(x=> x.Key, x=> x.Sum());
        }

        public static Dictionary<InventoryType, int> ToResourceDictionary(this MapContent content)
        {
            return new Dictionary<InventoryType, int>() { { new InventoryType() { ContentType = content.Type, ResourceType = content.ResourceType }, 1 } };
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

        /// <summary>
        /// Pure function, subtracts the amount of each InventoryType
        /// </summary>
        /// <param name="dictionary"></param>
        /// <param name="other"></param>
        /// <exception cref="InvalidOperationException">If it is impossible to subtract (values go below 0)</exception>
        /// <returns>A new dictionary with the subtracted values.</returns>
        public static Dictionary<InventoryType, int> SubtractInventory(this Dictionary<InventoryType, int> dictionary, Dictionary<InventoryType, int> other)
        {
            var subtracted = dictionary.SubtractDictionaries(other);
            if (!subtracted.Values.All(v => v >= 0))
                throw new InvalidOperationException();
            return subtracted;
        }



        public static List<DatabaseInventoryStorage> ToDatabaseStorage(this Dictionary<InventoryType, int> dictionary)
        {
            return dictionary.ToList().ConvertAll(v => new DatabaseInventoryStorage() { ContentType = v.Key.ContentType, ResourceType = v.Key.ResourceType, amount = v.Value }).ToList();
        }

        /// <summary>
        /// ASSERTS NO DUPLICATE KEYS IN LIST! (same content & resourcetype). Function is only used to convert dictionary to a database compatible custom type.
        /// </summary>
        /// <param name="list">List without duplicate Keys</param>
        /// <returns></returns>
        public static Dictionary<InventoryType, int> ToInventoryDictionary(this List<DatabaseInventoryStorage> list)
        {
            return list.ToDictionary(v => new InventoryType() { ContentType = v.ContentType, ResourceType = v.ResourceType }, v => v.amount);
        }

        public static bool QuestHasItemAsTarget(this Quest quest, MapContent contentToCheck)
        {
            if (quest.QuestLevel != contentToCheck.Type)
                return false;
            if (quest.TypeToPickUp != contentToCheck.ResourceType)
                return false;
            return true;
        }

        public static IObservable<bool> SpawnConditionMet(this IObservable<GPS> code, MapContentService service, List<Func<List<MapContent>, bool>> spawnCheckFunctions)
        {
            return code.Select(v => service.GetListMapUpdate(v.Lat, v.Lon)).Switch().Select(v =>
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
                QuestId = ObjectId.NewObjectId().ToByteArray(),
                QuestReward = new List<QuestReward>() { new QuestReward() { Amount = 10, ContentType = ContentType.QUESTREWARDPOINTS, ResourceType = ResourceType.NONE } }
            };
            return quest;
        }


        public static MapContent AsMapContent(this IUser user)
        {
            return new MapContent() { Id = user.UserId, Name = user.UserName, ResourceType = ResourceType.NONE, Type = ContentType.PLAYER, Location = null, CanBeLootedByPlayer = false };
        }

        public static MapContent AsMapContent(this Resource resource)
        {
            return new MapContent() { Id = resource.Id, Location = resource.Location, Name = resource.Name, ResourceType = resource.ResourceType, Type = resource.Type, CanBeLootedByPlayer = true };
        }

        public static ContentMessage AsMessage(this MapContent content)
        {
            return new ContentMessage() { Id = content.Id, Location = content.Location, Name = content.Name, ResourceType = content.ResourceType, Type = content.Type };
        }

        public static byte[] Hash(string value, byte[] salt)
        {
            return Hash(Encoding.UTF8.GetBytes(value), salt);
        }

        public static byte[] Hash(byte[] value, byte[] salt)
        {
            var pbkdf2 = new Rfc2898DeriveBytes(value, salt, 1000);
            return pbkdf2.GetBytes(20);
        }

        public static bool PasswordMatches(byte[] password, byte[] originalPassword, byte[] originalSalt)
        {
            var pbkdf2 = new Rfc2898DeriveBytes(password, originalSalt, 1000);
            var result = pbkdf2.GetBytes(20);
            return result.SequenceEqual(originalPassword);
        }

    }
}

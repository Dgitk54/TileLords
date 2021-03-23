using System;
using System.Collections.Generic;
using System.Text;
using DataModel.Server.Model;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Threading.Tasks;
using System.Linq;
using MongoDB.Driver.Linq;
using System.Diagnostics;
using DataModel.Common.GameModel;
using MongoDB.Bson.Serialization.Attributes;
using DataModel.Common.Messages;
using DataModel.Common;
using System.Linq.Expressions;

namespace DataModel.Server
{
    [BsonIgnoreExtraElements]
    public static class MongoDBFunctions
    {

        public static string UserDatabaseName => "Users";
        public static string MapContentDatabaseName => "MapContent";
        public static string InventoryDatabaseName => "Inventory";
        public static string QuestDatabaseName => "Quest";

        static MongoClient client = new MongoClient("mongodb://192.168.1.184:27017");


        public static void WipeAllDatabases()
        {
            client.DropDatabase(UserDatabaseName);
            client.DropDatabase(MapContentDatabaseName);
            client.DropDatabase(InventoryDatabaseName);
            client.DropDatabase(QuestDatabaseName);
        }

        public static void InitializeDataBases()
        {
            client.GetDatabase(UserDatabaseName);
            client.GetDatabase(MapContentDatabaseName);
            client.GetDatabase(InventoryDatabaseName);
            client.GetDatabase(QuestDatabaseName);
        }


        public static async Task<bool> InsertUser(User user)
        {

            var database = client.GetDatabase("Users");
            var collection = database.GetCollection<User>("users");

            var col = collection.AsQueryable();
            if (IsNameTaken(user.UserName, col))
            {
                return false;
            }
            await collection.InsertOneAsync(user);
            return true;
        }

        public static async Task<User> FindUserInDatabase(string name)
        {
            var database = client.GetDatabase("Users");
            var col = database.GetCollection<User>("users");
            var enumerable = await col.Find(v => v.UserName == name).ToListAsync();
            return enumerable.FirstOrDefault();

        }

        public static async Task<int> GetOnlineUsers()
        {
            var database = client.GetDatabase("Users");
            var col = database.GetCollection<User>("users");
            var enumerable = await col.Find(v => v.CurrentlyOnline).ToListAsync();
            return enumerable.Count();

        }



        public static async Task<BatchContentMessage> AreaContentAsMessageRequest(string location)
        {
            var nearbyCodes = LocationCodeTileUtility.GetTileSection(location, ServerFunctions.CLIENTVISIBILITY, ServerFunctions.CLIENTLOCATIONPRECISION);
            var database = client.GetDatabase("MapContent");
            var col = database.GetCollection<MapContent>("mapcontent");
            //unsupported filter//
            var all = await col.Find(Builders<MapContent>.Filter.Empty).ToListAsync();

            var enumerable = all.Where(v => nearbyCodes.Any(v2 => v2.Equals(v.Location)));
            var list = enumerable.ToList().ConvertAll(v => v.AsMessage());
            return new BatchContentMessage() { ContentList = list };

        }


        public static async Task<bool> AddQuestForUser(QuestContainer container)
        {
            var database = client.GetDatabase("Quest");
            var col = database.GetCollection<QuestContainer>("quests");
            await col.InsertOneAsync(container);
            return true;
        }


        public static async Task<MapContent> GetMapContentById(byte[] mapcontentid)
        {
            var database = client.GetDatabase("MapContent");
            var col = database.GetCollection<MapContent>("mapcontent");


            var filter = Builders<MapContent>.Filter.Eq<byte[]>(m => m.Id, mapcontentid);

            var enumerable = await col.Find(filter).ToListAsync();

            if (enumerable.Count() > 1)
                throw new Exception("Multiple objects with same ID in database");
            if (enumerable.Count() == 0)
            {
                return null;
            }
            var ret = enumerable.First();

            return ret;


        }

        public static async Task<bool> UpdateUserOnlineState(byte[] id, bool state)
        {
            var objId = new MongoDB.Bson.ObjectId(id);
            var database = client.GetDatabase("Users");
            var col = database.GetCollection<User>("users");
            var enumerable = await col.Find(v => v.UserId == objId).ToListAsync();
            if (enumerable.Count() > 1)
                throw new Exception("More than one user with same name");
            if (enumerable.Count() == 0)
                return false;
            var user = enumerable.First();
            user.CurrentlyOnline = state;

            var filter = Builders<User>.Filter.Eq<MongoDB.Bson.ObjectId>(m => m.UserId, user.UserId);

            var result = await col.ReplaceOneAsync(filter, user);
            return result.IsAcknowledged;

        }

        public static async Task<bool> UpdateUserOnlineState(string id, bool state)
        {
            var database = client.GetDatabase("Users");
            var col = database.GetCollection<User>("users");
            var enumerable = await col.Find(v => v.UserName == id).ToListAsync();
            if (enumerable.Count() > 1)
                throw new Exception("More than one user with same name");
            if (enumerable.Count() == 0)
                return false;
            var user = enumerable.First();
            user.CurrentlyOnline = state;
            var filter = Builders<User>.Filter.Eq<MongoDB.Bson.ObjectId>(m => m.UserId, user.UserId);

            var result = await col.ReplaceOneAsync(filter, user);
            return result.IsAcknowledged;
        }

        public static int ResetMapContent()
        {
            var database = client.GetDatabase("MapContent");
            var col = database.GetCollection<MapContent>("mapcontent");
            int players = (int)col.DeleteMany(v => v.Type == ContentType.PLAYER).DeletedCount;
            int resources = (int)col.DeleteMany(v => v.Type == ContentType.RESOURCE).DeletedCount;
            return players + resources;


        }

        public static async Task<List<QuestContainer>> GetQuestsForUser(byte[] userid)
        {
            var database = client.GetDatabase("Quest");
            var col = database.GetCollection<QuestContainer>("quests");
            var allPlayerQuests = await col.Find(v => v.OwnerId == userid).ToListAsync();
            if (allPlayerQuests.Count() == 0)
                return null;
            return allPlayerQuests.ToList();

        }

        public static async Task<bool> CreatePlayerInventory(byte[] playerId)
        {

            var database = client.GetDatabase("Inventory");
            var col = database.GetCollection<Inventory>("inventory");
            var enumerable = await col.Find(v => v.OwnerId == playerId).ToListAsync();

            if (enumerable.Count() > 1)
                throw new Exception("Multiple inventories for same player id");
            if (enumerable.Count() == 1)
                return false;

            var toInsert = new Inventory() { ContainerId = playerId, OwnerId = playerId, InventoryItems = new List<DatabaseInventoryStorage>(), StorageCapacity = 500 };
            await col.InsertOneAsync(toInsert);
            return true;

        }

        public static bool IsNameTaken(string name, IMongoQueryable<User> userList)
        {
            foreach (var user in userList)
            {
                if (user.UserName == name)
                {
                    return true;
                }
            }
            return false;
        }

        public static async Task<bool> RemoveQuestForUser(byte[] userId, byte[] questId)
        {
            var database = client.GetDatabase("Quest");
            var col = database.GetCollection<QuestContainer>("quests");

            var userQuests = await col.Find(v => v.OwnerId == userId).ToListAsync();
            var enumerable = userQuests.Where(e => e.Quest.QuestId.SequenceEqual(questId));
            if (enumerable.Count() > 1)
                throw new Exception("Invalid state in database");
            if (enumerable.Count() == 0)
                return false;

            var questContainer = enumerable.First();
            var result = await col.DeleteOneAsync(v => v.OwnerId == questContainer.OwnerId);
            return result.DeletedCount > 0;
        }


        public static async Task<MapContent> RemoveMapContent(byte[] contentId)
        {
            var database = client.GetDatabase("MapContent");
            var col = database.GetCollection<MapContent>("mapcontent");
            var enumerable = await col.Find(v => v.Id == contentId).ToListAsync();

            if (enumerable.Count() > 1)
                throw new Exception("Multiple objects with same ID in database");
            if (enumerable.Count() == 0)
                return null;
            var ret = enumerable.First();
            var deleted = await col.DeleteManyAsync(v => v.Id == contentId);
            Debug.Assert(deleted.DeletedCount == 1);
            return ret;
        }


        public static async Task<bool> RemoveContentFromInventory(byte[] inventoryId, byte[] ownerId, Dictionary<InventoryType, int> content)
        {
            var database = client.GetDatabase("Inventory");
            var col = database.GetCollection<Inventory>("inventory");
            var enumerable = await col.Find(v => v.OwnerId == inventoryId).ToListAsync();
            var containerRequest = enumerable.Where(v => v.ContainerId.SequenceEqual(ownerId));
            if (containerRequest.Count() > 1)
                throw new Exception("Multiple inventories for same player id");
            if (containerRequest.Count() == 0)
                return false;
            var inventory = containerRequest.First();
            var inventoryDictionary = inventory.InventoryItems.ToInventoryDictionary();

            Dictionary<InventoryType, int> subtractedResult = null;
            try
            {
                subtractedResult = inventoryDictionary.SubtractInventory(content);
            }
            catch (InvalidOperationException)
            {
                return false;
            }

            Debug.Assert(subtractedResult != null);
            inventory.InventoryItems = subtractedResult.ToDatabaseStorage();
            var filter = Builders<Inventory>.Filter.Eq<byte[]>(m => m.ContainerId, inventoryId);

            var result = await col.ReplaceOneAsync(filter, inventory);


            return result.IsAcknowledged;
        }


        public static async Task<bool> AddContentToPlayerInventory(byte[] inventoryId, Dictionary<InventoryType, int> content)
        {
            var database = client.GetDatabase("Inventory");
            var col = database.GetCollection<Inventory>("inventory");
            var enumerable = await col.Find(v => v.OwnerId == inventoryId).ToListAsync();
            var containerRequest = enumerable.Where(v => v.ContainerId.SequenceEqual(inventoryId));
            if (containerRequest.Count() > 1)
                throw new Exception("Multiple inventories for same player id");
            if (enumerable.Count() == 0)
            {
                await CreatePlayerInventory(inventoryId);
                return await AddContentToPlayerInventory(inventoryId, content);
            }
            else
            {
                //TODO: improve performance, operation in write lock!
                var inventory = containerRequest.First();
                var inventoryDictionary = inventory.InventoryItems.ToInventoryDictionary();

                content.ToList().ForEach(x =>
                {
                    int i = 0;
                    if (inventoryDictionary.TryGetValue(x.Key, out i))
                    {
                        inventoryDictionary[x.Key] = x.Value + i;
                    }
                    else
                    {
                        inventoryDictionary.Add(x.Key, x.Value);
                    }
                });
                inventory.InventoryItems = inventoryDictionary.ToDatabaseStorage();


                var filter = Builders<Inventory>.Filter.Eq<byte[]>(m => m.ContainerId, inventoryId);

                var result = await col.ReplaceOneAsync(filter, inventory);



                return result.IsAcknowledged;

            }
        }




        public static async Task<bool> RemoveContentAndAddToPlayer(byte[] playerId, byte[] mapcontentId)
        {

            //Check if the player can loot the item first with readonly db queries:
            var contentAsRead = await GetMapContentById(mapcontentId);
            if (contentAsRead == null)
            {
                Debug.WriteLine("content as read null");
                return false;
            }
            if (!contentAsRead.CanBeLootedByPlayer)
            {

                Debug.WriteLine("content not lootable");
                return false;

            }
            var playerCanLoot = ServerFunctions.PlayerCanLootObject(await GetQuestsForUser(playerId), contentAsRead);
            if (!playerCanLoot)
            {
                Debug.WriteLine("player cant loot");
                return false;
            }



            //var databaseMap = client.GetDatabase("MapData");
            //var databaseInventory = client.GetDatabase("Inventory");

            var content = await RemoveMapContent(mapcontentId);
            if (content == null)
            {
                Debug.WriteLine("content null");
                return false;
            }
            var asDictionary = content.ToResourceDictionary();
            return await AddContentToPlayerInventory(playerId, asDictionary);
        }



        public static async Task UpdateOrDeleteContent(MapContent content, string location)
        {
            var database = client.GetDatabase("MapContent");
            var col = database.GetCollection<MapContent>("mapcontent");
            var enumerable = await col.Find(v => v.Id == content.Id).ToListAsync();
            if (enumerable.Count() > 1)
                throw new Exception("Multiple objects with same ID in database");

            //No database entry and no location given, skip/ignore update
            if (enumerable.Count() == 0 && location == null)
            {
                return;
            }

            //received mapcontent with location but its not in database, insert into database
            if (enumerable.Count() == 0 && location != null)
            {
                content.Location = location;
                await col.InsertOneAsync(content);
                return;
            }

            var first = enumerable.First();

            //Delte value out of database, if it is still present.
            if (enumerable.Count() != 0 && location == null)
            {
                var deletedAmount = col.DeleteMany(v => v.Id == first.Id);
                return;
            }

            //Update value:
            first.Location = location;


            var filter = Builders<MapContent>.Filter.Eq<byte[]>(m => m.Id, content.Id);

            var result = await col.ReplaceOneAsync(filter, first);



            if (!result.IsAcknowledged)
                throw new Exception("Could not update entity");

            return;
        }
        public static async Task<bool> AddQuestForUser(byte[] userId, QuestContainer container)
        {
            var database = client.GetDatabase("Quest");
            var col = database.GetCollection<QuestContainer>("quests");
            await col.InsertOneAsync(container);
            return true;


        }

        public static async Task<List<MapContent>> AreaContentAsListRequest(string location)
        {
            var nearbyCodes = LocationCodeTileUtility.GetTileSection(location, ServerFunctions.CLIENTVISIBILITY, ServerFunctions.CLIENTLOCATIONPRECISION);
            var database = client.GetDatabase("MapContent");

            var col = database.GetCollection<MapContent>("mapcontent");
            var enumerable = await col.Find(v => nearbyCodes.Any(v2 => v2.Equals(v.Location))).ToListAsync();
            return enumerable.ToList();

        }

        public static async Task<Dictionary<InventoryType, int>> RequestInventory(byte[] requestOwnerId, byte[] targetId)
        {
            var database = client.GetDatabase("Inventory");
            var col = database.GetCollection<Inventory>("inventory");
            var getContainersWherePlayerHasInventory = await col.Find(v => v.OwnerId == requestOwnerId).ToListAsync();
            var getRequestedContainerInventory = getContainersWherePlayerHasInventory.Where(v => v.ContainerId.SequenceEqual(targetId));

            if (getRequestedContainerInventory.Count() > 1)
                throw new Exception("Multiple objects with same ID in database");
            if (getRequestedContainerInventory.Count() == 0)
            {
                //No user inventory has been set up yet
                if (requestOwnerId.SequenceEqual(targetId))
                {

                    await CreatePlayerInventory(requestOwnerId);
                    return await RequestInventory(requestOwnerId, targetId);
                }
                return null;
            }

            var inventory = getRequestedContainerInventory.First();
            return inventory.InventoryItems.ToInventoryDictionary();
        }



        public static async Task<bool> TurnInQuest(byte[] userId, byte[] questId)
        {

            var databaseQuest = client.GetDatabase("Quest");
            var databaseInventory = client.GetDatabase("Inventory");
            //Get write locks and loot it:

            var userQuests = await GetQuestsForUser(userId);
            var enumerable = userQuests.Where(e => e.Quest.QuestId.SequenceEqual(questId));
            var count = enumerable.Count();
            if (count > 1)
                throw new Exception("Error in queststate! Multiple Quests for same ID");
            if (count == 0)
                return false;
            var quest = enumerable.First();


            var currentUserInventory = await RequestInventory(userId, userId);
            var itemKey = quest.Quest.GetQuestItemDictionaryKey();
            var questItems = 0;
            var userHasQuestItems = currentUserInventory.TryGetValue(itemKey, out questItems);

            if (!userHasQuestItems)
                return false;
            if (questItems < quest.Quest.RequiredAmountForCompletion)
                return false;
            if (!await RemoveQuestForUser(userId, questId))
                return false;
            var subtractDictionary = new Dictionary<InventoryType, int>()
                { {itemKey, quest.Quest.RequiredAmountForCompletion} };
            var removeResult = await RemoveContentFromInventory(userId, userId, subtractDictionary);
            if (!removeResult)
                return false;
            var addReward = await AddContentToPlayerInventory(userId, quest.Quest.QuestReward.ToResourceDictionary());

            return addReward;

        }
    }

}
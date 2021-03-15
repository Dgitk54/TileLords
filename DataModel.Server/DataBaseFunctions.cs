using DataModel.Common;
using DataModel.Common.GameModel;
using DataModel.Common.Messages;
using DataModel.Server.Model;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace DataModel.Server
{
    public static class DataBaseFunctions
    {
        public static string UserDatabaseName => @"Users.db";
        public static string MapDatabaseName => @"MapData.db";
        public static string InventoryDatabaseName => @"Inventory.db";
        public static string QuestDatabaseName => @"Quest.db";
        public static void WipeAllDatabases()
        {
            if (File.Exists(UserDatabaseName))
                File.Delete(UserDatabaseName);
            if (File.Exists(MapDatabaseName))
                File.Delete(MapDatabaseName);
            if (File.Exists(InventoryDatabaseName))
                File.Delete(InventoryDatabaseName);
            if (File.Exists(QuestDatabaseName))
                File.Delete(QuestDatabaseName);
        }
        static ConnectionString QuestDatabaseWrite()
        {
            return new ConnectionString(QuestDatabaseName)
            {
                Connection = ConnectionType.Shared,
                ReadOnly = false
            };
        }
        static ConnectionString QuestDatabaseRead()
        {
            return new ConnectionString(QuestDatabaseName)
            {
                Connection = ConnectionType.Shared,
                ReadOnly = true
            };
        }
        static ConnectionString UserDatabaseWrite()
        {
            return new ConnectionString(UserDatabaseName)
            {
                Connection = ConnectionType.Shared,
                ReadOnly = false
            };
        }
        static ConnectionString UserDatabaseRead()
        {
            return new ConnectionString(UserDatabaseName)
            {
                Connection = ConnectionType.Shared,
                ReadOnly = true 
            };
        }

        static ConnectionString InventoryDatabaseWrite()
        {
            return new ConnectionString(InventoryDatabaseName)
            {
                Connection = ConnectionType.Shared,
                ReadOnly = false
            };
        }

        static ConnectionString InventoryDatabaseRead()
        {
            return new ConnectionString(InventoryDatabaseName)
            {
                Connection = ConnectionType.Shared,
                ReadOnly = true
            };
        }
        static ConnectionString MapDataRead()
        {
            return new ConnectionString(MapDatabaseName)
            {
                Connection = ConnectionType.Shared,
                ReadOnly = true
            };

        }
        static ConnectionString MapDataWrite()
        {
            return new ConnectionString(MapDatabaseName)
            {
                Connection = ConnectionType.Shared
            };
        }
        
        /// <summary>
        /// Creates all databases in the server enviroment.
        /// </summary>
        public static void InitializeDataBases()
        {
            using (var dataBase = new LiteDatabase(MapDataWrite()))
            {
                var col = dataBase.GetCollection<MapContent>("mapcontent");
                col.EnsureIndex(v => v.MapId);
                col.EnsureIndex(v => v.Location);
                col.EnsureIndex(v => v.Name);
                col.EnsureIndex(v => v.ResourceType);
                col.EnsureIndex(v => v.Type);
            }
            using (var dataBase = new LiteDatabase(InventoryDatabaseWrite()))
            {
                var col = dataBase.GetCollection<Inventory>("inventory");
                col.EnsureIndex(v => v.ContainerId);
                col.EnsureIndex(v => v.OwnerId);
                col.EnsureIndex(v => v.InventoryItems);
                col.EnsureIndex(v => v.StorageCapacity);
            }
            using (var dataBase = new LiteDatabase(UserDatabaseWrite()))
            {
                var col = dataBase.GetCollection<User>("users");
                col.EnsureIndex(v => v.AccountCreated);
                col.EnsureIndex(v => v.LastOnline);
                col.EnsureIndex(v => v.UserName);
                col.EnsureIndex(v => v.Salt);
                col.EnsureIndex(v => v.SaltedHash);
            }
            using (var dataBase = new LiteDatabase(QuestDatabaseWrite()))
            {
                var col = dataBase.GetCollection<QuestContainer>("quests");
                col.EnsureIndex(v => v.OwnerId);
                col.EnsureIndex(v => v.Quest);
                col.EnsureIndex(v => v.QuestCreatedOn);
                col.EnsureIndex(v => v.QuestHasExpired);
                col.EnsureIndex(v => v.QuestItemAliveTimeInSeconds);
                col.EnsureIndex(v => v.QuestItemsMaxAliveInQuestArea);
                col.EnsureIndex(v => v.QuestItemSpawnChancePerSecond);
            }
        }
        
        /// <summary>
        /// Function which removes content on the map identified by the id and transfers it to the player inventory.
        /// </summary>
        /// <param name="playerId"></param>
        /// <param name="mapcontentId"></param>
        /// <returns>true if action was successful</returns>
        public static bool RemoveContentAndAddToPlayer(byte[] playerId, byte[] mapcontentId)
        {
            //Check if the player can loot the item first with readonly db queries:
            var contentAsRead = GetMapContentById(mapcontentId);
            if (contentAsRead == null)
                return false;
            if (!contentAsRead.CanBeLootedByPlayer)
                return false;
            var playerCanLoot = ServerFunctions.PlayerCanLootObject(GetQuestsForUser(playerId), contentAsRead);
            if (!playerCanLoot)
                return false;


            //Get write locks and loot it:
            using (LiteDatabase mapData = new LiteDatabase(MapDataWrite()),
                              inventory = new LiteDatabase(InventoryDatabaseWrite()))
            {
                var content = RemoveMapContent(mapcontentId);
                if (content == null)
                {
                    return false;
                }
                var asDictionary = content.ToResourceDictionary();
                return AddContentToPlayerInventory(playerId, asDictionary);
            }
        }

        /// <summary>
        /// Finds a MapContent by the ID
        /// </summary>
        /// <param name="mapcontentid"></param>
        /// <returns>MapContent if any was found, null if not</returns>
        public static MapContent GetMapContentById(byte[] mapcontentid)
        {
            using (LiteDatabase dataBase = new LiteDatabase(MapDataRead()))
            {
                var col = dataBase.GetCollection<MapContent>("mapcontent");
                var enumerable = col.Find(v => v.MapId == mapcontentid);
                if (enumerable.Count() > 1)
                    throw new Exception("Multiple objects with same ID in database");
                if (enumerable.Count() == 0)
                    return null;
                var ret = enumerable.First();
                return ret;
            }
        }

        /// <summary>
        /// Turns a quest in, granting the user the rewards to their inventory.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="questId"></param>
        /// <returns>true if operation was successful</returns>
        public static bool TurnInQuest(byte[] userId, byte[] questId)
        {
            //Get write locks and loot it:
            using (LiteDatabase questData = new LiteDatabase(QuestDatabaseWrite()),
                              inventory = new LiteDatabase(InventoryDatabaseWrite()))
            {
                var userQuests = GetQuestsForUser(userId);
                var enumerable = userQuests.Where(e => e.Quest.QuestId.SequenceEqual(questId));
                var count = enumerable.Count();
                if (count > 1)
                    throw new Exception("Error in queststate! Multiple Quests for same ID");
                if (count == 0)
                    return false;
                var quest = enumerable.First();

                
                var currentUserInventory = RequestInventory(userId, userId);
                var itemKey = quest.Quest.GetQuestItemDictionaryKey();
                var questItems = 0;
                var userHasQuestItems = currentUserInventory.TryGetValue(itemKey, out questItems);
                
                if (!userHasQuestItems)
                    return false;
                if (questItems < quest.Quest.RequiredAmountForCompletion)
                    return false;
                if (!RemoveQuestForUser(userId, questId))
                    return false;
                var subtractDictionary = new Dictionary<InventoryType, int>()
                { {itemKey, quest.Quest.RequiredAmountForCompletion} };
                var removeResult = RemoveContentFromInventory(userId, userId, subtractDictionary);
                if (!removeResult)
                    return false;
                var addReward = AddContentToPlayerInventory(userId, quest.Quest.QuestReward.ToResourceDictionary());

                return addReward;
            }
        }

        /// <summary>
        /// Adds a quest for a given userid, identified by the byte id.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="container"></param>
        /// <returns>true if operation was successful</returns>
        public static bool AddQuestForUser(byte[] userId, QuestContainer container)
        {
            using (LiteDatabase dataBase = new LiteDatabase(QuestDatabaseWrite()))
            {
                var col = dataBase.GetCollection<QuestContainer>("quests");
                col.Insert(container);
                return true;
            }

        }
        
        /// <summary>
        /// Removes a quest for a given user, both identified by their byte[] ids
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="questId"></param>
        /// <returns>true if operation was successful</returns>
        public static bool RemoveQuestForUser(byte[] userId, byte[] questId)
        {
            using (LiteDatabase dataBase = new LiteDatabase(QuestDatabaseWrite()))
            {
                var col = dataBase.GetCollection<QuestContainer>("quests");

                var userQuests = col.Find(v => v.OwnerId == userId);
                var enumerable = userQuests.Where(e => e.Quest.QuestId.SequenceEqual(questId));
                if (enumerable.Count() > 1)
                    throw new Exception("Invalid state in database");
                if (enumerable.Count() == 0)
                    return false;

                var questContainer = enumerable.First();
                return col.Delete(questContainer.IdLite);
            }
        }

        /// <summary>
        /// returns a List of QuestContainer for the playerid
        /// </summary>
        /// <param name="userid">The ID of the player</param>
        /// <returns>List of QuestContainers, null if none found.</returns>
        public static List<QuestContainer> GetQuestsForUser(byte[] userid)
        {
            using (LiteDatabase dataBase = new LiteDatabase(QuestDatabaseRead()))
            {
                var col = dataBase.GetCollection<QuestContainer>("quests");
                var allPlayerQuests = col.Find(v => v.OwnerId == userid);
                if (allPlayerQuests.Count() == 0)
                    return null;
                return allPlayerQuests.ToList();
            }
        }

        /// <summary>
        /// Function which removes content in the database.
        /// </summary>
        /// <param name="contentId">The ID of the content to remove</param>
        /// <returns>MapContent if successful, null if content could not be removed</returns>
        public static MapContent RemoveMapContent(byte[] contentId)
        {
            using (var dataBase = new LiteDatabase(MapDataWrite()))
            {
                var col = dataBase.GetCollection<MapContent>("mapcontent");
                var enumerable = col.Find(v => v.MapId == contentId);

                if (enumerable.Count() > 1)
                    throw new Exception("Multiple objects with same ID in database");
                if (enumerable.Count() == 0)
                    return null;
                var ret = enumerable.First();
                var deleted = col.DeleteMany(v => v.MapId == contentId);
                Debug.Assert(deleted == 1);
                return ret;
            }

        }

        /// <summary>
        /// Removes content by subtracting the current inventory identified by the two byte[] ids with the given content
        /// </summary>
        /// <param name="inventoryId">ID of the Container, for example a Town</param>
        /// <param name="ownerId">Owner of the Inventory</param>
        /// <param name="content">Contentdictionary to subtract</param>
        /// <returns>true if operation suceeded, false if the inventory does not contain enough items to subtract the given content dictionary</returns>
        public static bool RemoveContentFromInventory(byte[] inventoryId, byte[] ownerId, Dictionary<InventoryType, int> content)
        {
            using (var dataBase = new LiteDatabase(InventoryDatabaseWrite()))
            {
                var col = dataBase.GetCollection<Inventory>("inventory");
                var enumerable = col.Find(v => v.OwnerId == inventoryId);
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
                return col.Update(inventory);
            }
        }

        /// <summary>
        /// Adds the given content dictionary to a player inventory
        /// </summary>
        /// <param name="inventoryId"></param>
        /// <param name="content"></param>
        /// <returns>true if the transaction was successful</returns>
        public static bool AddContentToPlayerInventory(byte[] inventoryId, Dictionary<InventoryType, int> content)
        {
            using (var dataBase = new LiteDatabase(InventoryDatabaseWrite()))
            {
                var col = dataBase.GetCollection<Inventory>("inventory");
                var enumerable = col.Find(v => v.OwnerId == inventoryId);
                var containerRequest = enumerable.Where(v => v.ContainerId.SequenceEqual(inventoryId));
                if (containerRequest.Count() > 1)
                    throw new Exception("Multiple inventories for same player id");
                if (enumerable.Count() == 0)
                {
                    CreatePlayerInventory(inventoryId);
                    return AddContentToPlayerInventory(inventoryId, content);
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
                    return col.Update(inventory);
                }
            }
        }
        
        /// <summary>
        /// Creates a player inventory for a player id
        /// </summary>
        /// <param name="playerId"></param>
        /// <returns>true if the transaction was successful</returns>
        public static bool CreatePlayerInventory(byte[] playerId)
        {
            using (var dataBase = new LiteDatabase(InventoryDatabaseWrite()))
            {
                var col = dataBase.GetCollection<Inventory>("inventory");
                var enumerable = col.Find(v => v.OwnerId == playerId);

                if (enumerable.Count() > 1)
                    throw new Exception("Multiple inventories for same player id");
                if (enumerable.Count() == 1)
                    return false;

                var toInsert = new Inventory() { ContainerId = playerId, OwnerId = playerId, InventoryItems = new List<DatabaseInventoryStorage>(), StorageCapacity = 500 };
                col.Insert(toInsert);
                return true;
            }
        }
        public static Dictionary<InventoryType, int> RequestInventory(byte[] requestOwnerId, byte[] targetId)
        {
            using (var dataBase = new LiteDatabase(InventoryDatabaseRead()))
            {
                var col = dataBase.GetCollection<Inventory>("inventory");
                var getContainersWherePlayerHasInventory = col.Find(v => v.OwnerId == requestOwnerId);
                var getRequestedContainerInventory = getContainersWherePlayerHasInventory.Where(v => v.ContainerId.SequenceEqual(targetId));

                if (getRequestedContainerInventory.Count() > 1)
                    throw new Exception("Multiple objects with same ID in database");
                if (getRequestedContainerInventory.Count() == 0)
                {
                    //No user inventory has been set up yet
                    if (requestOwnerId.SequenceEqual(targetId))
                    {

                        CreatePlayerInventory(requestOwnerId);
                        return RequestInventory(requestOwnerId, targetId);
                    }
                    return null;
                }

                var inventory = getRequestedContainerInventory.First();
                return inventory.InventoryItems.ToInventoryDictionary();
            }
        }

        /// <summary>
        /// Inserts/Updates or deletes the content in the database if no location is provided
        /// </summary>
        /// <param name="content">MapContent to insert/update</param>
        /// <param name="location">The current location of the MapContent, if null this function will delete the mapcontent</param>
        public static void UpdateOrDeleteContent(MapContent content, string location)
        {
            using (var dataBase = new LiteDatabase(MapDataWrite()))
            {
                var col = dataBase.GetCollection<MapContent>("mapcontent");
                var enumerable = col.Find(v => v.MapId == content.MapId);
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
                    col.Insert(content);
                    return;
                }

                var first = enumerable.First();

                //Delte value out of database, if it is still present.
                if (enumerable.Count() != 0 && location == null)
                {
                    var deletedAmount = col.DeleteMany(v => v.MapId == first.MapId);
                    return;
                }

                //Update value:
                first.Location = location;
                var result = col.Update(first);
                if (!result)
                    throw new Exception("Could not update entity");
            }
        }
        
        /// <summary>
        /// Function which wipes all the mapcontent in the map database.
        /// </summary>
        /// <returns>amount of items wiped</returns>
        public static int ResetMapContent()
        {
            using (var dataBase = new LiteDatabase(MapDataWrite()))
            {
                var col = dataBase.GetCollection<MapContent>("mapcontent");
                int players = col.DeleteMany(v => v.Type == ContentType.PLAYER);
                int resources = col.DeleteMany(v => v.Type == ContentType.RESOURCE);
                return players + resources;
            }

        }

        /// <summary>
        /// Requests a list of visible mapcontent around a location.
        /// </summary>
        /// <param name="location">Middle Location</param>
        /// <returns>List of visible content. </returns>
        public static List<MapContent> AreaContentAsListRequest(string location)
        {
            var nearbyCodes = LocationCodeTileUtility.GetTileSection(location, ServerFunctions.CLIENTVISIBILITY, ServerFunctions.CLIENTLOCATIONPRECISION);
            using (var dataBase = new LiteDatabase(MapDataRead()))
            {
                var col = dataBase.GetCollection<MapContent>("mapcontent");
                var enumerable = col.Find(v => nearbyCodes.Any(v2 => v2.Equals(v.Location)));
                return enumerable.ToList();
            }
        }

        /// <summary>
        /// See <see cref="AreaContentAsListRequest"/> wrapped in a batchcontentmessage
        /// </summary>
        /// <param name="location">Middle Location</param>
        /// <returns>Message with the List containing all content. </returns>
        public static BatchContentMessage AreaContentAsMessageRequest(string location)
        {
            var nearbyCodes = LocationCodeTileUtility.GetTileSection(location, ServerFunctions.CLIENTVISIBILITY, ServerFunctions.CLIENTLOCATIONPRECISION);
            using (var dataBase = new LiteDatabase(MapDataRead()))
            {
                var col = dataBase.GetCollection<MapContent>("mapcontent");
                var enumerable = col.Find(v => nearbyCodes.Any(v2 => v2.Equals(v.Location)));
                var list = enumerable.ToList().ConvertAll(v => v.AsMessage());
                return new BatchContentMessage() { ContentList = list };
            }
        }

        /// <summary>
        /// Function which creates a new user account
        /// </summary>
        /// <param name="name"></param>
        /// <param name="password"></param>
        /// <returns>false if username is already taken</returns>
        public static bool CreateAccount(User user)
        {
            using (var dataBase = new LiteDatabase(UserDatabaseWrite()))
            {
                var col = dataBase.GetCollection<User>("users");
                if (NameTaken(user.UserName, col))
                    return false;
                col.Insert(user);
                return true;
            }
        }

        /// <summary>
        /// Function which finds a user identified with the name
        /// </summary>
        /// <param name="name"></param>
        /// <returns>User object, null if none found</returns>
        public static User FindUserInDatabase(string name)
        {
            using (var dataBase = new LiteDatabase(UserDatabaseRead()))
            {
                var col = dataBase.GetCollection<User>("users");
                var enumerable = col.Find(v => v.UserName == name);
                return enumerable.FirstOrDefault();
            }
        }
        
        /// <summary>
        /// Function which updates the user online state in the database
        /// </summary>
        /// <param name="id">ID of the user</param>
        /// <param name="state">Current online state</param>
        /// <returns>true if action was successful.</returns>
        public static bool UpdateUserOnlineState(byte[] id, bool state)
        {
            var objId = new LiteDB.ObjectId(id);
            using (var dataBase = new LiteDatabase(UserDatabaseWrite()))
            {
                var col = dataBase.GetCollection<User>("users");
                var enumerable = col.Find(v => v.UserIdLite == objId);
                if (enumerable.Count() > 1)
                    throw new Exception("More than one user with same name");
                if (enumerable.Count() == 0)
                    return false;
                var user = enumerable.First();
                user.CurrentlyOnline = state;
                return col.Update(user);
            }
        }
        
        /// <summary>
        /// Function which returns the amount of currently online users.
        /// </summary>
        /// <returns>Amount of online users as int</returns>
        public static int GetOnlineUsers()
        {
            using (var dataBase = new LiteDatabase(UserDatabaseRead()))
            {
                var col = dataBase.GetCollection<User>("users");
                var enumerable = col.Find(v => v.CurrentlyOnline);
                return enumerable.Count();
            }
        }

        /// <summary>
        /// Works similar as <see cref="UpdateUserOnlineState(byte[], bool)"/> but queries the user with the name.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="state"></param>
        /// <returns>true if action was successful.</returns>
        public static bool UpdateUserOnlineState(string id, bool state)
        {
            using (var dataBase = new LiteDatabase(UserDatabaseWrite()))
            {
                var col = dataBase.GetCollection<User>("users");
                var enumerable = col.Find(v => v.UserName == id);
                if (enumerable.Count() > 1)
                    throw new Exception("More than one user with same name");
                if (enumerable.Count() == 0)
                    return false;
                var user = enumerable.First();
                user.CurrentlyOnline = state;
                return col.Update(user);
            }
        }

        /// <summary>
        /// Checks the collection if the username is already taken.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="col">Collection to check</param>
        /// <returns>true if the name is already taken.</returns>
        public static bool NameTaken(string name, ILiteCollection<User> col)
        {
            return col.Find(v => v.UserName == name).Any();
        }
    }
}

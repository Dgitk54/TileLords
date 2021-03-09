﻿using DataModel.Common;
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
        public static void InitializeDataBases()
        {
            using (var dataBase = new LiteDatabase(MapDataWrite()))
            {
                var col = dataBase.GetCollection<MapContent>("mapcontent");
                col.EnsureIndex(v => v.Id);
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

        public static MapContent GetMapContentById(byte[] mapcontentid)
        {
            using (LiteDatabase dataBase = new LiteDatabase(MapDataRead()))
            {
                var col = dataBase.GetCollection<MapContent>("mapcontent");
                var enumerable = col.Find(v => v.Id == mapcontentid);
                if (enumerable.Count() > 1)
                    throw new Exception("Multiple objects with same ID in database");
                if (enumerable.Count() == 0)
                    return null;
                var ret = enumerable.First();
                return ret;
            }
        }

        public static bool AddQuestForUser(byte[] userId, QuestContainer container)
        {
            using (LiteDatabase dataBase = new LiteDatabase(QuestDatabaseWrite()))
            {
                var col = dataBase.GetCollection<QuestContainer>("quests");
                col.Insert(container);
                return true;
            }

        }

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
                var enumerable = col.Find(v => v.Id == contentId);

                if (enumerable.Count() > 1)
                    throw new Exception("Multiple objects with same ID in database");
                if (enumerable.Count() == 0)
                    return null;
                var ret = enumerable.First();
                var deleted = col.DeleteMany(v => v.Id == contentId);
                Debug.Assert(deleted == 1);
                return ret;
            }

        }

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
                var enumerable = col.Find(v => v.Id == content.Id);
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
                    var deletedAmount = col.DeleteMany(v => v.Id == first.Id);
                    return;
                }

                //Update value:
                first.Location = location;
                var result = col.Update(first);
                if (!result)
                    throw new Exception("Could not update entity");
            }
        }
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

        public static List<MapContent> AreaContentAsListRequest(string location)
        {
            using (var dataBase = new LiteDatabase(MapDataRead()))
            {
                var col = dataBase.GetCollection<MapContent>("mapcontent");
                var nearbyCodes = LocationCodeTileUtility.GetTileSection(location, ServerFunctions.CLIENTVISIBILITY, ServerFunctions.CLIENTLOCATIONPRECISION);
                var enumerable = col.Find(v => nearbyCodes.Any(v2 => v2.Equals(v.Location)));
                return enumerable.ToList();
            }
        }


        public static BatchContentMessage AreaContentAsMessageRequest(string location)
        {
            using (var dataBase = new LiteDatabase(MapDataRead()))
            {
                var col = dataBase.GetCollection<MapContent>("mapcontent");
                var nearbyCodes = LocationCodeTileUtility.GetTileSection(location, ServerFunctions.CLIENTVISIBILITY, ServerFunctions.CLIENTLOCATIONPRECISION);
                var enumerable = col.Find(v => nearbyCodes.Any(v2 => v2.Equals(v.Location)));
                var list = enumerable.ToList().ConvertAll(v => v.AsMessage());
                return new BatchContentMessage() { ContentList = list };

            }
        }

        public static bool CreateAccount(string name, string password)
        {

            using (var dataBase = new LiteDatabase(UserDatabaseWrite()))
            {
                var col = dataBase.GetCollection<User>("users");
                if (NameTaken(name, col))
                    return false;

                byte[] salt;
                new RNGCryptoServiceProvider().GetBytes(salt = new byte[16]);

                var hashedpass = ServerFunctions.Hash(password, salt);

                var userForDb = new User
                {
                    AccountCreated = DateTime.Now,
                    Salt = salt,
                    SaltedHash = hashedpass,
                    UserName = name
                };

                col.Insert(userForDb);
                return true;
            }
        }

        public static User FindUserInDatabase(string name)
        {
            using (var dataBase = new LiteDatabase(UserDatabaseRead()))
            {
                try
                {
                    var col = dataBase.GetCollection<User>("users");
                    if (col.Find(v => v.UserName == name).Any())
                        return col.Find(v => v.UserName == name).First();
                    return null;


                }
                catch (System.IO.FileNotFoundException)
                {
                    using (var dbwrite = new LiteDatabase(MapDataWrite()))
                    {
                        var col = dbwrite.GetCollection<User>("users");
                        col.EnsureIndex(v => v.AccountCreated);
                        col.EnsureIndex(v => v.LastOnline);
                        col.EnsureIndex(v => v.UserName);
                        col.EnsureIndex(v => v.Salt);
                        col.EnsureIndex(v => v.SaltedHash);

                        return null;
                    }
                }

            }
        }

        public static bool NameTaken(string name, ILiteCollection<User> col)
        {
            return col.Find(v => v.UserName == name).Any();
        }
    }
}

using DataModel.Common;
using DataModel.Common.Messages;
using DataModel.Server.Model;
using DataModel.Server.Services;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace DataModel.Server
{
    public static class DataBaseFunctions
    {
        public static string UserDatabaseName { get { return @"Users.db"; } }
        public static string MapDatabaseName { get { return @"MapData.db"; } }
        public static string InventoryDatabaseName { get { return @"Inventory.db"; } }
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
        /// Function which removes content in the database.
        /// </summary>
        /// <param name="contentId">The ID of the content to remove</param>
        /// <returns>MapContent if successful, null if content could not be removed</returns>
        public static MapContent RemoveMapContent(byte[] contentId)
        {
            using (var dataBase = new LiteDatabase(MapDataWrite()))
            {
                var col = dataBase.GetCollection<MapContent>("mapcontent");
                col.EnsureIndex(v => v.Id);
                col.EnsureIndex(v => v.Location);
                col.EnsureIndex(v => v.Name);
                col.EnsureIndex(v => v.ResourceType);
                col.EnsureIndex(v => v.Type);
                var enumerable = col.Find(v => v.Id == contentId);

                if (enumerable.Count() > 1)
                    throw new Exception("Multiple objects with same ID in database");
                if (enumerable.Count() == 0)
                    return null;
                return enumerable.First();
            }

        }
        public static bool AddContentToPlayerInventory(byte[] inventoryId, Dictionary<ResourceType, int> content)
        {
            using (var dataBase = new LiteDatabase(InventoryDatabaseWrite()))
            {
                var col = dataBase.GetCollection<Inventory>("inventory");
                col.EnsureIndex(v => v.ContainerId);
                col.EnsureIndex(v => v.OwnerId);
                col.EnsureIndex(v => v.ResourceDictionary);
                col.EnsureIndex(v => v.StorageCapacity);
                var enumerable = col.Find(v => v.OwnerId == inventoryId);
                var containerRequest = enumerable.Where(v => v.ContainerId.SequenceEqual(inventoryId));
                if (containerRequest.Count() > 1)
                    throw new Exception("Multiple inventories for same player id");
                if (enumerable.Count() == 0)
                {
                    CreatePlayerInventory(inventoryId);
                    AddContentToPlayerInventory(inventoryId, content);
                }
                else
                {
                    var inventory = containerRequest.First();
                    content.ToList().ForEach(x =>
                    {
                        int oldval = inventory.ResourceDictionary[x.Key];
                        inventory.ResourceDictionary[x.Key] = x.Value + oldval;
                    });
                    return col.Update(inventory);
                }
                return false;
            }


        }
        public static bool CreatePlayerInventory(byte[] playerId)
        {
            using (var dataBase = new LiteDatabase(InventoryDatabaseWrite()))
            {
                var col = dataBase.GetCollection<Inventory>("inventory");
                col.EnsureIndex(v => v.ContainerId);
                col.EnsureIndex(v => v.OwnerId);
                col.EnsureIndex(v => v.ResourceDictionary);
                col.EnsureIndex(v => v.StorageCapacity);
                var enumerable = col.Find(v => v.OwnerId == playerId);

                if (enumerable.Count() > 1)
                    throw new Exception("Multiple inventories for same player id");
                if (enumerable.Count() == 1)
                    return false;

                var toInsert = new Inventory() { ContainerId = playerId, OwnerId = playerId, ResourceDictionary = new Dictionary<Common.Messages.ResourceType, int>(), StorageCapacity = 500 };
                col.Insert(toInsert);
                return true;
            }
        }
        public static Dictionary<ResourceType, int> RequestInventory(byte[] requestOwnerId, byte[] targetId)
        {
            using (var dataBase = new LiteDatabase(InventoryDatabaseRead()))
            {
                var col = dataBase.GetCollection<Inventory>("inventory");
                col.EnsureIndex(v => v.ContainerId);
                col.EnsureIndex(v => v.OwnerId);
                col.EnsureIndex(v => v.ResourceDictionary);
                col.EnsureIndex(v => v.StorageCapacity);
                var enumerable = col.Find(v => v.OwnerId == requestOwnerId);
                var containerRequest = enumerable.Where(v => v.ContainerId.SequenceEqual(targetId));

                if (containerRequest.Count() > 1)
                    throw new Exception("Multiple objects with same ID in database");
                if (containerRequest.Count() == 0)
                    return null;
                var inventory = containerRequest.First();
                return inventory.ResourceDictionary;
            }
        }

        /// <summary>
        /// Updates or deletes the content in the database if no location is provided
        /// </summary>
        /// <param name="content">MapContent to insert/update</param>
        /// <param name="location">The current location of the MapContent, if null this function will delete the mapcontent</param>
        public static void UpdateOrDeleteContent(MapContent content, string location)
        {
            using (var dataBase = new LiteDatabase(MapDataWrite()))
            {
                var col = dataBase.GetCollection<MapContent>("mapcontent");
                col.EnsureIndex(v => v.Id);
                col.EnsureIndex(v => v.Location);
                col.EnsureIndex(v => v.Name);
                col.EnsureIndex(v => v.ResourceType);
                col.EnsureIndex(v => v.Type);

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
                col.EnsureIndex(v => v.Id);
                col.EnsureIndex(v => v.Location);
                col.EnsureIndex(v => v.Name);
                col.EnsureIndex(v => v.ResourceType);
                col.EnsureIndex(v => v.Type);

                int players = col.DeleteMany(v => v.Type == ContentType.PLAYER);
                int resources = col.DeleteMany(v => v.Type == ContentType.RESSOURCE);
                return players + resources;
            }

        }

        public static List<MapContent> AreaContentAsListRequest(string location)
        {
            //TODO: FIX BUG, WORKS ONLY IN WRITE ONLY MODE!
            using (var dataBase = new LiteDatabase(MapDataWrite()))
            {
                try
                {
                    var col = dataBase.GetCollection<MapContent>("mapcontent");
                    col.EnsureIndex(v => v.Id);
                    col.EnsureIndex(v => v.Location);
                    col.EnsureIndex(v => v.Name);
                    col.EnsureIndex(v => v.ResourceType);
                    col.EnsureIndex(v => v.Type);

                    var nearbyCodes = LocationCodeTileUtility.GetTileSection(location, ServerFunctions.CLIENTVISIBILITY, ServerFunctions.CLIENTLOCATIONPRECISION);

                    var enumerable = col.Find(v => nearbyCodes.Any(v2 => v2.Equals(v.Location)));

                    return enumerable.ToList();
                }
                catch (FileNotFoundException e)
                {

                    using (var dbwrite = new LiteDatabase(MapDataWrite()))
                    {
                        var col = dbwrite.GetCollection<MapContent>("mapcontent");
                        col.EnsureIndex(v => v.Id);
                        col.EnsureIndex(v => v.Location);
                        col.EnsureIndex(v => v.Name);
                        col.EnsureIndex(v => v.ResourceType);
                        col.EnsureIndex(v => v.Type);
                        return null;
                    }
                }
            }
        }


        public static BatchContentMessage AreaContentAsMessageRequest(string location)
        {
            //TODO: FIX BUG, WORKS ONLY IN WRITE ONLY MODE!
            using (var dataBase = new LiteDatabase(MapDataWrite()))
            {
                try
                {
                    var col = dataBase.GetCollection<MapContent>("mapcontent");
                    col.EnsureIndex(v => v.Id);
                    col.EnsureIndex(v => v.Location);
                    col.EnsureIndex(v => v.Name);
                    col.EnsureIndex(v => v.ResourceType);
                    col.EnsureIndex(v => v.Type);

                    var nearbyCodes = LocationCodeTileUtility.GetTileSection(location, ServerFunctions.CLIENTVISIBILITY, ServerFunctions.CLIENTLOCATIONPRECISION);

                    var enumerable = col.Find(v => nearbyCodes.Any(v2 => v2.Equals(v.Location)));

                    var list = enumerable.ToList().ConvertAll(v => v.AsMessage());
                    return new BatchContentMessage() { ContentList = list };
                }
                catch (FileNotFoundException e)
                {

                    using (var dbwrite = new LiteDatabase(MapDataWrite()))
                    {
                        var col = dbwrite.GetCollection<MapContent>("mapcontent");
                        col.EnsureIndex(v => v.Id);
                        col.EnsureIndex(v => v.Location);
                        col.EnsureIndex(v => v.Name);
                        col.EnsureIndex(v => v.ResourceType);
                        col.EnsureIndex(v => v.Type);
                        return null;
                    }
                }

            }
        }

        public static bool CreateAccount(string name, string password)
        {

            using (var dataBase = new LiteDatabase(UserDatabaseWrite()))
            {
                var col = dataBase.GetCollection<User>("users");
                col.EnsureIndex(v => v.AccountCreated);
                col.EnsureIndex(v => v.LastOnline);
                col.EnsureIndex(v => v.UserName);
                col.EnsureIndex(v => v.Salt);
                col.EnsureIndex(v => v.SaltedHash);

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
                catch (System.IO.FileNotFoundException e)
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
            => col.Find(v => v.UserName == name).Any();
    }
}

using DataModel.Common;
using DataModel.Common.Messages;
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
        

        static ConnectionString DataBaseRead()
        {
            return new ConnectionString(@"MyData.db")
            {
                Connection = ConnectionType.Shared,
                ReadOnly = true
            };

        }
        static ConnectionString DataBasePath()
        {
            return new ConnectionString(@"MyData.db")
            {
                Connection = ConnectionType.Shared
            };
        }


        /// <summary>
        /// Updates or deletes the content in the database if no location is provided
        /// </summary>
        /// <param name="content"></param>
        /// <param name="location"></param>
        public static void UpdateOrDeleteContent(MapContent content, string location)
        {
            using (var dataBase = new LiteDatabase(DataBasePath()))
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

                if (enumerable.Count() == 0 && location != null)
                {
                    content.Location = location;
                    col.Insert(content);
                    return;
                }

                var first = enumerable.First();
                if (location == null)
                {
                    var deletedAmount = col.DeleteMany(v => v.Id == first.Id);
                    Debug.Assert(deletedAmount == 1);
                    return;
                }
                first.Location = location;
                var result = col.Update(first);
                if (!result)
                    throw new Exception("Could not update entity");
            }
        }

        
        public static BatchContentMessage AreaContentRequest(string location)
        {
            //TODO: FIX BUG, WORKS ONLY IN WRITE ONLY MODE!
            using (var dataBase = new LiteDatabase(DataBasePath()))
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
                    
                    using (var dbwrite = new LiteDatabase(DataBasePath()))
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

            using (var dataBase = new LiteDatabase(DataBasePath()))
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
            using (var dataBase = new LiteDatabase(DataBaseRead()))
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
                    using (var dbwrite = new LiteDatabase(DataBasePath()))
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

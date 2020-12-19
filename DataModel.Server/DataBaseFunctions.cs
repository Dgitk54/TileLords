using DataModel.Common;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

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


        static public bool CreateAccount(this UserRegisterEvent user)
        {

            using (var dataBase = new LiteDatabase(DataBasePath()))
            {
                var col = dataBase.GetCollection<User>("users");
                col.EnsureIndex(v => v.AccountCreated);
                col.EnsureIndex(v => v.Inventory);
                col.EnsureIndex(v => v.LastOnline);
                col.EnsureIndex(v => v.LastPostion);
                col.EnsureIndex(v => v.UserName);
                col.EnsureIndex(v => v.Salt);
                col.EnsureIndex(v => v.SaltedHash);

                if (NameTaken(user.Name, col))
                    return false;

                byte[] salt;
                new RNGCryptoServiceProvider().GetBytes(salt = new byte[16]);

                var password = ServerFunctions.Hash(user.Password, salt);

                var userForDb = new User
                {
                    AccountCreated = DateTime.Now,
                    Salt = salt,
                    SaltedHash = password,
                    UserName = user.Name
                };

                col.Insert(userForDb);
                return true;
            }



        }

        public static Tile LookUpTile(PlusCode code)
        {
            using (var db = new LiteDatabase(DataBaseRead()))
            {

                var largeCode = code;
                if (largeCode.Precision == 10)
                    DataModelFunctions.ToLowerResolution(code, 8);

                try
                {
                    var col = db.GetCollection<Tile>("tiles");

                    var results = col.Find(v => v.PlusCode.Code == code.Code);

                    using (var dbWrite = new LiteDatabase(DataBasePath()))
                    {
                        if (results == null || results.Count() == 0)
                        {
                            Debug.WriteLine("put " + code.Code + " in");
                            var col2 = dbWrite.GetCollection<Tile>("tiles");

                            col2.EnsureIndex(v => v.MiniTiles);
                            col2.EnsureIndex(v => v.PlusCode);
                            col2.EnsureIndex(v => v.Ttype);

                            var created = TileGenerator.GenerateArea(largeCode, 0);
                            var tile = created[0];
                            var dbVal = col2.Insert(tile);
                            tile.Id = dbVal.AsInt32;
                            return tile;
                        }
                    }


                    if (results.Count() > 1)
                        throw new Exception("More than one object for same index!");
                    return results.First();
                }
                catch (System.IO.FileNotFoundException)
                {
                    using (var dbwrite = new LiteDatabase(DataBasePath()))
                    {

                      
                        Debug.WriteLine("test");
                        var col2 = dbwrite.GetCollection<Tile>("tiles");

                        col2.EnsureIndex(v => v.MiniTiles);
                        col2.EnsureIndex(v => v.PlusCode);
                        col2.EnsureIndex(v => v.Ttype);

                        var created = TileGenerator.GenerateArea(largeCode, 0);
                        var tile = created[0];
                        var dbVal = col2.Insert(tile);
                        tile.Id = dbVal.AsInt32;
                        return tile;
                    }

                }



            }

        }



        static public User InDataBase(string name)
        {
            using (var dataBase = new LiteDatabase(DataBaseRead()))
            {
                try
                {
                    var col = dataBase.GetCollection<User>("users");
                    if (col.Find(v => v.UserName == name).Any())
                        return col.Find(v => v.UserName == name).First();
                    return null;


                } catch(System.IO.FileNotFoundException e)
                {
                    using (var dbwrite = new LiteDatabase(DataBasePath()))
                    {
                        var col = dbwrite.GetCollection<User>("users");
                        col.EnsureIndex(v => v.AccountCreated);
                        col.EnsureIndex(v => v.Inventory);
                        col.EnsureIndex(v => v.LastOnline);
                        col.EnsureIndex(v => v.LastPostion);
                        col.EnsureIndex(v => v.UserName);
                        col.EnsureIndex(v => v.Salt);
                        col.EnsureIndex(v => v.SaltedHash);

                        return null;
                    }
                }
               
            }



        }


        static public bool NameTaken(string name, ILiteCollection<User> col)
            => col.Find(v => v.UserName == name).Any();
    }
}

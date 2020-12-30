using DataModel.Common;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace DataModel.Server
{
    public static class DataBaseFunctions
    {
        private static Mutex mut = new Mutex(false);

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

        public static void UpdateTile(MiniTile miniTile)
        {

        }


        public static MiniTile LookupMiniTile(PlusCode code)
        {
            var tile = LookUpWithGenerateTile(code);
            return TileUtility.GetMiniTile(code, tile.MiniTiles);
        }

        public static MiniTile LookupOnlyMiniTile(PlusCode code)
        {
            var largeCode = code;
            if (largeCode.Precision == 10)
                largeCode = code.ToLowerResolution(8);
            var tile = LookupOnly(largeCode);
            if (tile == null)
                return null;
            return TileUtility.GetMiniTile(code, tile.MiniTiles);
        }


        public static Tile CreateTile(PlusCode code)
        {
            using (var dbWrite = new LiteDatabase(DataBasePath()))
            {
                var colWrite = dbWrite.GetCollection<Tile>("tiles");
                colWrite.EnsureIndex(v => v.MiniTiles);
                colWrite.EnsureIndex(v => v.PlusCode);
                colWrite.EnsureIndex(v => v.Ttype);

                //Ensure in lock no tiles have been added.
                var resultInLock = colWrite.Find(v => v.PlusCode.Code == code.Code);
                if (resultInLock == null || resultInLock.Count() == 0)
                {
                    var created = TileGenerator.GenerateArea(code, 0);
                    var tile = created[0];
                    var dbVal = colWrite.Insert(tile);
                    tile.Id = dbVal.AsInt32;
                    return tile;
                }
                if (resultInLock.Count() > 1)
                    throw new Exception("More than one object for same index!");
                return resultInLock.First();
            }
        }

        public static Tile LookupOnly(PlusCode code)
        {
            var largeCode = code;
            if (largeCode.Precision == 10)
                largeCode = code.ToLowerResolution(8);
            using (var db = new LiteDatabase(DataBaseRead()))
            {
                var col = db.GetCollection<Tile>("tiles");
                var results = col.Find(v => v.PlusCode.Code == code.Code);
                if (results.Count() > 1)
                    throw new Exception("More than one object for same index!");

                if (!results.Any())
                    return null;
                return results.First();
            }
        }

        public static Tile LookUpWithGenerateTile(PlusCode code)
        {
            var largeCode = code;
            if (largeCode.Precision == 10)
                largeCode = code.ToLowerResolution(8);
            try
            {
                mut.WaitOne();
                using (var db = new LiteDatabase(DataBaseRead()))
                {
                    var col = db.GetCollection<Tile>("tiles");
                    var results = col.Find(v => v.PlusCode.Code == code.Code);


                    if (results == null || results.Count() == 0)
                    {
                        Tile resultInLock = null;
                        using (var dbWrite = new LiteDatabase(DataBasePath()))
                        {
                            var colWrite = dbWrite.GetCollection<Tile>("tiles");
                            colWrite.EnsureIndex(v => v.MiniTiles);
                            colWrite.EnsureIndex(v => v.PlusCode);
                            colWrite.EnsureIndex(v => v.Ttype);

                            //Ensure in lock no tiles have been added.
                            var tiles = colWrite.Find(v => v.PlusCode.Code == code.Code);
                            if (tiles == null || tiles.Count() == 0)
                            {
                                var created = TileGenerator.GenerateArea(largeCode, 0);
                                var tile = created[0];
                                var dbVal = colWrite.Insert(tile);
                                tile.Id = dbVal.AsInt32;
                                resultInLock = tile;
                            }
                        }
                        mut.ReleaseMutex();
                        return resultInLock;
                    }

                    var count = results.Count();
                    var asList = results.ToList();
                    if (results.Count() > 1)
                        throw new Exception("More than one object for same index!");
                    mut.ReleaseMutex();
                    return results.First();

                }
            }
            catch (System.IO.FileNotFoundException)
            {
                using (var dbwrite = new LiteDatabase(DataBasePath()))
                {
                    var writeCol = dbwrite.GetCollection<Tile>("tiles");

                    writeCol.EnsureIndex(v => v.MiniTiles);
                    writeCol.EnsureIndex(v => v.PlusCode);
                    writeCol.EnsureIndex(v => v.Ttype);

                    return LookUpWithGenerateTile(code);
                }
            }
        }





        public static User InDataBase(string name)
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

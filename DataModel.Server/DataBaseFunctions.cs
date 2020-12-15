using DataModel.Common;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace DataModel.Server
{
    public static class DataBaseFunctions
    {
        
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
            using (var db = new LiteDatabase(DataBasePath()))
            {

                var largeCode = code;
                if (largeCode.Precision == 10)
                    DataModelFunctions.ToLowerResolution(code, 8);

                var col = db.GetCollection<Tile>("tiles");
                col.EnsureIndex(v => v.MiniTiles);
                col.EnsureIndex(v => v.PlusCode);
                col.EnsureIndex(v => v.Ttype);
                var results = col.Find(v => v.PlusCode.Code == code.Code);
                if (results.Count() == 0)
                {
                    ;
                    var created = TileGenerator.GenerateArea(largeCode, 0);
                    var tile = created[0];

                    var dbVal = col.Insert(tile);
                    tile.Id = dbVal.AsInt32;
                    return tile;
                }
                if (results.Count() > 1)
                    throw new Exception("More than one object for same index!");
                return results.First();



            }

        }



        static public User InDataBase(string name)
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

                if (col.Find(v => v.UserName == name).Any())
                    return col.Find(v => v.UserName == name).First();
                return null;
            }


                
        }


        static public bool NameTaken(string name, ILiteCollection<User> col)
            => col.Find(v => v.UserName == name).Any();
    }
}

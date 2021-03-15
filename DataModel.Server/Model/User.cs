
using MongoDB.Bson;
using System;

namespace DataModel.Server.Model
{
    public class User : IUser
    {
        byte[] IUser.UserId => UserId.ToByteArray();

        public MongoDB.Bson.ObjectId UserId { get; set; }

        public LiteDB.MongoDB.Bson.ObjectId UserIdLite { get; set; }

        public string UserName { get; set; }

        public DateTime LastOnline { get; set; }

        public DateTime AccountCreated { get; set; }

        public byte[] Salt { get; set; }

        public byte[] SaltedHash { get; set; }

        public bool CurrentlyOnline { get; set; }
    }
}

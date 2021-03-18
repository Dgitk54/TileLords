using System;

namespace GameServer
{
    public class User : IUser
    {
        byte[] IUser.UserId => UserId.ToByteArray();
        public Guid UserId { get; set; }

        public string UserName { get; set; }

        public DateTime LastOnline { get; set; }

        public DateTime AccountCreated { get; set; }

        public byte[] Salt { get; set; }

        public byte[] SaltedHash { get; set; }

        public bool CurrentlyOnline { get; set; }
    }
}

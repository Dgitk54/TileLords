﻿using System;
using System.Collections.Generic;
using System.Text;
using LiteDB;

namespace DataModel.Server.Services
{
    public class User : IUser
    {
        byte[] IUser.UserId => UserId.ToByteArray();
        public ObjectId UserId { get; set; }

        public string UserName { get ; set; }

        public DateTime LastOnline { get; set; }

        public DateTime AccountCreated { get; set; }

        public byte[] Salt { get; set; }

        public byte[] SaltedHash { get; set; }

        
    }
}
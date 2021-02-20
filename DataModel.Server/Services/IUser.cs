using System;
using System.Collections.Generic;
using System.Text;
using LiteDB;

namespace DataModel.Server.Services
{
    public interface IUser
    {
        byte[] UserId { get; } 
        string UserName { get; set; }
    }
}

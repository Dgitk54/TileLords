using System;
using System.Collections.Generic;
using System.Text;
using LiteDB;

namespace DataModel.Server.Model
{
    public interface IUser
    {
        byte[] UserId { get; } 
        string UserName { get; set; }
    }
}

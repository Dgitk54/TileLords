using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.Server
{
    public class Person
    {
        public MongoDB.Bson.ObjectId Id { get; set; }
        public string Name { get; set; }
    }
}

using DataModel.Common.Messages;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.Server.Model
{
    /// <summary>
    /// Class wrapping up user inventory in a different representation since the database (bson) can not contain complex types as keys in dictionaries/keyvaluepairs.
    /// </summary>
    public class DatabaseInventoryStorage
    {
        public ContentType ContentType { get; set; }
        public ResourceType ResourceType { get; set; }
        public int amount { get; set; }
    }
}

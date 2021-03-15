using DataModel.Common.GameModel;
using DataModel.Common.Messages;
using LiteDB;
using MongoDB.Bson.Serialization.Attributes;

namespace DataModel.Server.Model
{
    [BsonIgnoreExtraElements]
    public class MapContent
    {
        
        public ContentType Type { get; set; }

        public ResourceType ResourceType { get; set; }

        public byte[] MapId { get; set; }

        public string Name { get; set; }

        public string Location { get; set; }

        public bool CanBeLootedByPlayer { get; set; }
    }
}

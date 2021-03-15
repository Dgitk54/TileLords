using DataModel.Common.GameModel;
using DataModel.Common.Messages;
using LiteDB;

namespace DataModel.Server.Model
{
    public class MapContent
    {
        public MongoDB.Bson.ObjectId MapContentId { get; set; }

        public ContentType Type { get; set; }

        public ResourceType ResourceType { get; set; }

        public byte[] Id { get; set; }

        public string Name { get; set; }

        public string Location { get; set; }

        public bool CanBeLootedByPlayer { get; set; }
    }
}

using DataModel.Common.GameModel;
using DataModel.Common.Messages;
using MessagePack;

namespace DataModel.Server.Model
{
    [MessagePackObject]
    public class MapContent
    {
        
        [Key(0)]
        public ContentType Type { get; set; }

        [Key(1)]
        public ResourceType ResourceType { get; set; }

        [Key(2)]
        public byte[] Id { get; set; }

        [Key(3)]
        public string Name { get; set; }

        [Key(4)]
        public string Location { get; set; }

        [Key(5)]
        public bool CanBeLootedByPlayer { get; set; }
    }
}

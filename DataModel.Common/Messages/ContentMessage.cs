using DataModel.Common.GameModel;
using MessagePack;

namespace DataModel.Common.Messages
{

    [MessagePackObject]
    public class ContentMessage : IMessage
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


        public override string ToString()
        {
            if (Type == ContentType.RESOURCE)
            {
                return ResourceType + "" + " at " + TownNames.PlusCodeToTileName(new PlusCode(Location, 10)) + " " + Location;
            }
            else if (Type == ContentType.PLAYER)
            {
                if (Name != null)
                {
                    return "Player: " + " " + Name + " at " + TownNames.PlusCodeToTileName(new PlusCode(Location, 10)) + " " + Location;
                }
                else
                {
                    return "Player: " + " unknown Name";
                }
            }
            else if (Type == ContentType.RESOURCE)
            {
                if (Name != null)
                {
                    return "Quest: " + " " + Name + " at " + TownNames.PlusCodeToTileName(new PlusCode(Location, 10)) + " " + Location;
                }
                else
                {
                    return "Quest: " + " unknown Name";
                }

            }
            return null;
        }
    }
}

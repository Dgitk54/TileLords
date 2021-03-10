using DataModel.Common.GameModel;

namespace DataModel.Common.Messages
{
    public class ContentMessage : IMessage
    {

        public ContentType Type { get; set; }

        public ResourceType ResourceType { get; set; }

        public byte[] Id { get; set; }

        public string Name { get; set; }

        public string Location { get; set; }


        public override string ToString()
        {
            if (Type == ContentType.RESOURCE)
            {
                return ResourceType + "" + " at " + TileUtility.PlusCodeToTileName(new PlusCode(Location, 10)) + " " + Location;
            }
            else if (Type == ContentType.PLAYER)
            {
                if (Name != null)
                {
                    return "Player: " + " " + Name + " at " + TileUtility.PlusCodeToTileName(new PlusCode(Location, 10)) + " " + Location;
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
                    return "Quest: " + " " + Name + " at " + TileUtility.PlusCodeToTileName(new PlusCode(Location, 10)) + " " + Location;
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

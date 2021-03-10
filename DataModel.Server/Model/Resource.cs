using DataModel.Common.GameModel;
using DataModel.Common.Messages;

namespace DataModel.Server.Model
{
    public class Resource
    {
        public ContentType Type { get; set; }

        public ResourceType ResourceType { get; set; }

        public byte[] Id { get; set; }

        public string Name { get; set; }

        public string Location { get; set; }
    }
}

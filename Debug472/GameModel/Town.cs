using DataModel.Common.Messages;

namespace DataModel.Common.GameModel
{
    public class Town
    {
        public byte[] TownId { get; set; }
        public ContentType TownContentType { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public int TownDecayState { get; set; }
    }
}

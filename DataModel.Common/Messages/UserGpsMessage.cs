using MessagePack;

namespace DataModel.Common.Messages
{

    [MessagePackObject]
    public class UserGpsMessage : IMessage
    {

        [Key(0)]
        public double Lat { get; set; }

        [Key(1)]
        public double Lon { get; set; }
    }
}

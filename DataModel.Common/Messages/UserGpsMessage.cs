using MessagePack;

namespace DataModel.Common.Messages
{
    public class UserGpsMessage : IMessage
    {

        public double Lat { get; set; }


        public double Lon { get; set; }
    }
}

using MessagePack;

namespace DataModel.Common.Messages
{
    public class MapContentTransactionMessage : IMessage
    {
        public MessageType MessageType { get; set; }
        public MessageState MessageState { get; set; }
        public byte[] MapContentId { get; set; }

        public override string ToString()
        {
            return base.ToString() + MessageType + " " + MessageState + "";
        }
    }
}

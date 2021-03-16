using MessagePack;

namespace DataModel.Common.Messages
{

    [MessagePackObject]
    public class MapContentTransactionMessage : IMessage
    {
        [Key(0)]
        public MessageType MessageType { get; set; }
        [Key(1)]
        public MessageState MessageState { get; set; }
        [Key(2)]
        public byte[] MapContentId { get; set; }

        public override string ToString()
        {
            return base.ToString() + MessageType + " " + MessageState + "";
        }
    }
}

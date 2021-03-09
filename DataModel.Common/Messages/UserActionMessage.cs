namespace DataModel.Common.Messages
{

    public class UserActionMessage : IMessage
    {

        public MessageType MessageType { get; set; }


        public MessageContext MessageContext { get; set; }


        public MessageState MessageState { get; set; }


        public MessageInfo MessageInfo { get; set; }

        public byte[] AdditionalInfo { get; set; }
    }
}

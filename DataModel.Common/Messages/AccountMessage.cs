using MessagePack;

namespace DataModel.Common.Messages
{
    public class AccountMessage : IMessage
    {
        public MessageContext Context { get; set; }

        public string Name { get; set; }

        public string Password { get; set; }
    }
}

using DataModel.Common.GameModel;
using MessagePack;
using System.Collections.Generic;

namespace DataModel.Common.Messages
{
    public class ActiveUserQuestsMessage : IMessage
    {
        public MessageType MessageType { get; set; }
        public MessageState MessageState { get; set; }
        public List<Quest> CurrentUserQuests { get; set; }
    }
}

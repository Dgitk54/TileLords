using DataModel.Common.GameModel;
using MessagePack;
using System.Collections.Generic;

namespace DataModel.Common.Messages
{
    [MessagePackObject]
    public class ActiveUserQuestsMessage : IMessage
    {
        [Key(0)]
        public MessageType MessageType { get; set; }
        [Key(1)]
        public MessageState MessageState { get; set; }
        [Key(2)]
        public List<Quest> CurrentUserQuests { get; set; }
    }
}

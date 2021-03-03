using System;
using System.Collections.Generic;
using System.Text;
using DataModel.Common.GameModel;

namespace DataModel.Common.Messages
{
    public class QuestMessage
    {
        public MessageType MessageType { get; set; }
        public MessageState MessageState { get; set; }
        public List<Quest> CurrentUserQuests { get; set; }
    }
}

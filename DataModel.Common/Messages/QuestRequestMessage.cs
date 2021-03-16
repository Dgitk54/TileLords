using DataModel.Common.GameModel;
using MessagePack;

namespace DataModel.Common.Messages
{
    /// <summary>
    /// Used to request quests from town if the ID is given, or generates new level 1 quests which do not require a town + turn in point
    /// </summary>
    public class QuestRequestMessage : IMessage
    {
        public byte[] QuestContainerId { get; set; } // if null => requests level 1 quest
        public MessageType MessageType { get; set; }
        public MessageState MessageState { get; set; }
        public Quest Quest { get; set; }
    }
}

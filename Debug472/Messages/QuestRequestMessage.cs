using DataModel.Common.GameModel;
using MessagePack;

namespace DataModel.Common.Messages
{
    /// <summary>
    /// Used to request quests from town if the ID is given, or generates new level 1 quests which do not require a town + turn in point
    /// </summary>

    [MessagePackObject]
    public class QuestRequestMessage : IMessage
    {
        [Key(0)]
        public byte[] QuestContainerId { get; set; } // if null => requests level 1 quest
        [Key(1)]
        public MessageType MessageType { get; set; }
        [Key(2)]
        public MessageState MessageState { get; set; }
        [Key(3)]
        public Quest Quest { get; set; }
    }
}

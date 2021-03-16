using MessagePack;

namespace DataModel.Common.Messages
{
    /// <summary>
    /// Used to finish quests and trade in the resources into reward points
    /// </summary>

    [MessagePackObject]
    public class TurnInQuestMessage : IMessage
    {
        [Key(0)]
        public MessageType MessageType { get; set; }
        [Key(1)]
        public MessageState MessageState { get; set; }
        [Key(2)]
        public byte[] QuestId { get; set; }
    }
}

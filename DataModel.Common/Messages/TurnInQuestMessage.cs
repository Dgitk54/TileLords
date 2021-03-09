namespace DataModel.Common.Messages
{
    /// <summary>
    /// Used to finish quests and trade in the resources into reward points
    /// </summary>
    public class TurnInQuestMessage : IMessage
    {
        public MessageType MessageType { get; set; }
        public MessageState MessageState { get; set; }
        public byte[] QuestId { get; set; }
    }
}

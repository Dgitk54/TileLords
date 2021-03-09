using DataModel.Common.Messages;
using System;

namespace DataModel.Common.GameModel
{
    public class Quest
    {
        public byte[] QuestOriginId { get; set; } //Town the quest started from
        public byte[] QuestId { get; set; } //Quest ID 
        public ContentType QuestLevel { get; set; }
        public ResourceType TypeToPickUp { get; set; }
        public int RequiredAmountForCompletion { get; set; }
        public string QuestTargetLocation { get; set; }
        public string QuestTurninLocation { get; set; }
        public int AreaRadiusFromLocation { get; set; }
        public DateTime ExpiringDate { get; set; }
        public QuestReward QuestReward { get; set; }
    }
}

using DataModel.Common.Messages;
using MessagePack;
using System;
using System.Collections.Generic;

namespace DataModel.Common.GameModel
{
    [MessagePackObject]
    public class Quest
    {
        [Key(0)]
        public byte[] QuestOriginId { get; set; } //Town the quest started from
        [Key(1)]
        public byte[] QuestId { get; set; } //Quest ID 
        [Key(2)]
        public ContentType QuestLevel { get; set; }
        [Key(3)]
        public ResourceType TypeToPickUp { get; set; }
        [Key(4)]
        public int RequiredAmountForCompletion { get; set; }
        [Key(5)]
        public string QuestTargetLocation { get; set; }
        [Key(6)]
        public string QuestTurninLocation { get; set; }
        [Key(7)]
        public int AreaRadiusFromLocation { get; set; }
        [Key(8)]
        public DateTime ExpiringDate { get; set; }
        [Key(9)]
        public List<QuestReward> QuestReward { get; set; }
    }
}

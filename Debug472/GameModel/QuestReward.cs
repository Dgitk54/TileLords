using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.Common.GameModel
{
    [MessagePackObject]
    public class QuestReward
    {
        [Key(0)]
        public ContentType ContentType { get; set; }
        [Key(1)]
        public ResourceType ResourceType { get; set; }
        [Key(2)]
        public int Amount { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.Common.GameModel
{
    public class QuestReward
    {
        public ContentType ContentType { get; set; }
        public ResourceType ResourceType { get; set; }
        public int Amount { get; set; }
    }
}

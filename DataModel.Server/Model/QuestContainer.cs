using DataModel.Common.GameModel;
using System;

namespace DataModel.Server.Model
{
    public class QuestContainer
    {
        public byte[] OwnerId { get; set; }  //Player having the Quest
        public DateTime QuestCreatedOn { get; set; }
        public bool QuestHasExpired { get; set; }
        public double QuestItemSpawnChancePerSecond { get; set; }
        public int QuestItemsMaxAliveInQuestArea { get; set; }
        public int QuestItemAliveTimeInSeconds { get; set; }
        public Quest Quest { get; set; }
    }
}

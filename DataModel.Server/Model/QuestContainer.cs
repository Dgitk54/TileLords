﻿using DataModel.Common.GameModel;
using MongoDB.Bson;
using System;

namespace DataModel.Server.Model
{
    public class QuestContainer
    {
        public MongoDB.Bson.ObjectId Id { get; set; }
        public LiteDB.MongoDB.Bson.ObjectId IdLite { get; set; }
        public byte[] OwnerId { get; set; }  //Player having the Quest
        public DateTime QuestCreatedOn { get; set; }
        public bool QuestHasExpired { get; set; }
        public double QuestItemSpawnChancePerSecond { get; set; }
        public int QuestItemsMaxAliveInQuestArea { get; set; }
        public int QuestItemAliveTimeInSeconds { get; set; }
        public Quest Quest { get; set; }
    }
}

﻿using DataModel.Common.GameModel;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace DataModel.Server.Model
{
    /// <summary>
    /// Class wrapping a quest containing server internal Questinformation 
    /// </summary>
    [BsonIgnoreExtraElements]
    public class QuestContainer
    {
        public MongoDB.Bson.ObjectId Id { get; set; }
        public byte[] OwnerId { get; set; }  //Player having the Quest
        public DateTime QuestCreatedOn { get; set; }
        public bool QuestHasExpired { get; set; }
        public double QuestItemSpawnChancePerSecond { get; set; }
        public int QuestItemsMaxAliveInQuestArea { get; set; }
        public int QuestItemAliveTimeInSeconds { get; set; }
        public Quest Quest { get; set; }
    }
}

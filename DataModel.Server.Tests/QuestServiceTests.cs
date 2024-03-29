﻿using DataModel.Common;
using DataModel.Common.GameModel;
using DataModel.Server.Model;
using DataModel.Server.Services;
using MongoDB.Bson;
using NUnit.Framework;
using System.Collections.Generic;
using System.Reactive.Linq;

namespace DataModel.Server.Tests
{
    [TestFixture]
    public class QuestServiceTests
    {
        [SetUp]
        public void Setup()
        {
            MongoDBFunctions.WipeAllDatabases();
        }

        [TearDown]
        public void TearDown()
        {
        }

        [Test]
        public void CanCreateLevel1Quests()
        {
            var service = new QuestService();

            IUser user1 = new User()
            {
                UserId = ObjectId.GenerateNewId(),
                UserName = "TestUser",

            };

            var startLocation = new PlusCode("8FX9XW2F+9X", 10);

            var generatedQuest = service.GenerateNewQuest(user1, null, startLocation.Code).Take(1).Wait();
            Assert.IsTrue(generatedQuest != null);

            var questsRequest = service.RequestActiveQuests(user1.UserId).Take(1).Wait();

            Assert.IsTrue(questsRequest.Count == 1);
        }


        [Test]
        public void CanTurnInQuests()
        {
            var service = new QuestService();

            IUser user1 = new User()
            {
                UserId = ObjectId.GenerateNewId(),
                UserName = "TestUser",

            };

            var startLocation = new PlusCode("8FX9XW2F+9X", 10);

            var generatedQuest = service.GenerateNewQuest(user1, null, startLocation.Code).Take(1).Wait();
            Assert.IsTrue(generatedQuest != null);

            var questsRequest = service.RequestActiveQuests(user1.UserId).Take(1).Wait();

            Assert.IsTrue(questsRequest.Count == 1);

            var questItemKey = questsRequest[0].Quest.GetQuestItemDictionaryKey();
            var doubleQuestItemsDictionary = new Dictionary<InventoryType, int>()
                { {questItemKey, questsRequest[0].Quest.RequiredAmountForCompletion * 2} };


            //for testing purposes:
            MongoDBFunctions.AddContentToPlayerInventory(user1.UserId, doubleQuestItemsDictionary).Wait();
            var result = service.TurnInQuest(user1, questsRequest[0].Quest.QuestId).Take(1).Wait();
            Assert.IsTrue(result);

            //After turn in, the user inventory should have at least 2 inventorytype keys with some values:
            var userInventoryTask = MongoDBFunctions.RequestInventory(user1.UserId, user1.UserId);
            userInventoryTask.Wait();
            var userInventoryResult = userInventoryTask.Result;
            Assert.IsTrue(userInventoryResult.Keys.Count == 2);

            //User should not have any quests:
            var newQuestRequest = service.RequestActiveQuests(user1.UserId).Take(1).Wait();
            Assert.IsTrue(newQuestRequest == null);

        }



    }
}

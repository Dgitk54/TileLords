using DataModel.Common;
using DataModel.Server.Model;
using DataModel.Server.Services;
using LiteDB;
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
            DataBaseFunctions.WipeAllDatabases();
            DataBaseFunctions.InitializeDataBases();
        }

        [TearDown]
        public void TearDown()
        {
            DataBaseFunctions.WipeAllDatabases();
        }

        [Test]
        public void CanCreateLevel1Quests()
        {
            var service = new QuestService();

            IUser user1 = new User()
            {
                UserId = ObjectId.NewObjectId(),
                UserName = "TestUser",

            };

            var startLocation = new PlusCode("8FX9XW2F+9X", 10);

            var response = new List<QuestContainer>();

            var generatedQuest = service.GenerateNewQuest(user1, null, startLocation.Code).Take(1).Wait();
            Assert.IsTrue(generatedQuest != null);

            var questsRequest = service.RequestActiveQuests(user1.UserId).Take(1).Wait();

            Assert.IsTrue(questsRequest.Count == 1);
        }



    }
}

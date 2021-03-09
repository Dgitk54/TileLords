using DataModel.Common;
using DataModel.Server.Model;
using LiteDB;
using NUnit.Framework;
using System.Linq;

namespace DataModel.Server.Tests
{
    [TestFixture]
    public class DatabaseTests
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
        public void CanAddAndRemoveFromInventory()
        {
            IUser user1 = new User()
            {
                UserId = ObjectId.NewObjectId(),
                UserName = "TestUser",

            };

            MapContent randomContent = ServerFunctions.GetRandomNonQuestResource().AsMapContent();
            var startLocation = new PlusCode("8FX9XW2F+9X", 10);
            randomContent.Location = startLocation.Code;

            DataBaseFunctions.UpdateOrDeleteContent(randomContent, startLocation.Code);
            var result = DataBaseFunctions.RemoveContentAndAddToPlayer(user1.UserId, randomContent.Id);

            Assert.IsTrue(result);
            var userInventory = DataBaseFunctions.RequestInventory(user1.UserId, user1.UserId);
            Assert.IsTrue(userInventory.Count == 1);

            var removeResult = DataBaseFunctions.RemoveContentFromInventory(user1.UserId, user1.UserId, randomContent.ToResourceDictionary());
            Assert.IsTrue(removeResult);

            userInventory = DataBaseFunctions.RequestInventory(user1.UserId, user1.UserId);
            Assert.IsTrue(userInventory.Values.All(v => v == 0));
        }


    }
}

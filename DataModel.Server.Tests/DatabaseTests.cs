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
            MongoDBFunctions.WipeAllDatabases();
            MongoDBFunctions.InitializeDataBases();
        }

        [TearDown]
        public void TearDown()
        {
            MongoDBFunctions.WipeAllDatabases();
        }

        [Test]
        public void CanAddAndRemoveFromInventory()
        {
            IUser user1 = new User()
            {
                UserIdLite = ObjectId.NewObjectId(),
              

                UserName = "TestUser",

            };

            MapContent randomContent = ServerFunctions.GetRandomNonQuestResource().AsMapContent();
            var startLocation = new PlusCode("8FX9XW2F+9X", 10);
            randomContent.Location = startLocation.Code;

            MongoDBFunctions.UpdateOrDeleteContent(randomContent, startLocation.Code);
            var result = MongoDBFunctions.RemoveContentAndAddToPlayer(user1.UserId, randomContent.MapId).Result;

            Assert.IsTrue(result);
            var userInventory = MongoDBFunctions.RequestInventory(user1.UserId, user1.UserId).Result;
            Assert.IsTrue(userInventory.Count == 1);

            var removeResult = MongoDBFunctions.RemoveContentFromInventory(user1.UserId, user1.UserId, randomContent.ToResourceDictionary()).Result;
            Assert.IsTrue(removeResult);

            userInventory = MongoDBFunctions.RequestInventory(user1.UserId, user1.UserId).Result;
            Assert.IsTrue(userInventory.Values.All(v => v == 0));
        }
        [Test]
        public void CanCreateUserLoginAndLogOut()
        {
           // var user = MongoDBFunctions.CreateAccount("test", "test");
            var findUser = MongoDBFunctions.FindUserInDatabase("test").Result;
            Assert.IsTrue(findUser != null);
            var logOn = MongoDBFunctions.UpdateUserOnlineState(findUser.UserId.ToByteArray(), true).Result;
            Assert.IsTrue(logOn);
            var updatedUser = MongoDBFunctions.FindUserInDatabase("test").Result;
            Assert.IsTrue(updatedUser != null);
            Assert.IsTrue(updatedUser.CurrentlyOnline);

            var logOff = MongoDBFunctions.UpdateUserOnlineState(findUser.UserId.ToByteArray(), false).Result;
            Assert.IsTrue(logOff);
            updatedUser = MongoDBFunctions.FindUserInDatabase("test").Result;
            Assert.IsTrue(updatedUser != null);
            Assert.IsTrue(!updatedUser.CurrentlyOnline);
        }

    }
}

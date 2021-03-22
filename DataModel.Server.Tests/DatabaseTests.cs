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
            LiteDBDatabaseFunctions.WipeAllDatabases();
            LiteDBDatabaseFunctions.InitializeDataBases();
        }

        [TearDown]
        public void TearDown()
        {
            LiteDBDatabaseFunctions.WipeAllDatabases();
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

            LiteDBDatabaseFunctions.UpsertOrDeleteContent(randomContent, startLocation.Code);
            var result = LiteDBDatabaseFunctions.RemoveContentAndAddToPlayer(user1.UserId, randomContent.Id);

            Assert.IsTrue(result);
            var userInventory = LiteDBDatabaseFunctions.RequestInventory(user1.UserId, user1.UserId);
            Assert.IsTrue(userInventory.Count == 1);

            var removeResult = LiteDBDatabaseFunctions.RemoveContentFromInventory(user1.UserId, user1.UserId, randomContent.ToResourceDictionary());
            Assert.IsTrue(removeResult);

            userInventory = LiteDBDatabaseFunctions.RequestInventory(user1.UserId, user1.UserId);
            Assert.IsTrue(userInventory.Values.All(v => v == 0));
        }
        [Test]
        public void CanCreateUserLoginAndLogOut()
        {
            User user = new User() { UserName = "test" };
            var createAccount = LiteDBDatabaseFunctions.CreateAccount(user);
            Assert.IsTrue(createAccount);
            var findUser = LiteDBDatabaseFunctions.FindUserInDatabase("test");
            Assert.IsTrue(findUser != null);
            var logOn = LiteDBDatabaseFunctions.UpdateUserOnlineState(findUser.UserId.ToByteArray(), true);
            Assert.IsTrue(logOn);
            var updatedUser = LiteDBDatabaseFunctions.FindUserInDatabase("test");
            Assert.IsTrue(updatedUser != null);
            Assert.IsTrue(updatedUser.CurrentlyOnline);

            var logOff = LiteDBDatabaseFunctions.UpdateUserOnlineState(findUser.UserId.ToByteArray(), false);
            Assert.IsTrue(logOff);
            updatedUser = LiteDBDatabaseFunctions.FindUserInDatabase("test");
            Assert.IsTrue(updatedUser != null);
            Assert.IsTrue(!updatedUser.CurrentlyOnline);
        }

    }
}

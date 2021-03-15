using DataModel.Common;
using DataModel.Server.Model;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Threading.Tasks;
using System.Linq;
using MongoDB.Driver.Linq;
using NUnit.Framework;
using System.Diagnostics;
using MongoDB.Bson.Serialization.Attributes;

namespace DataModel.Server.Tests
{
    [TestFixture]
  
    public class MongoDBTests
    {
        [SetUp]
        public void Setup()
        {
            DatabaseMongoDBFunctions.WipeAllDatabases();
            DatabaseMongoDBFunctions.InitializeDataBases();
        }

        [TearDown]
        public void TearDown()
        {
            DatabaseMongoDBFunctions.WipeAllDatabases();
        }

        [Test]
        public  void CanAddAndRemoveFromInventory()
        {
            IUser user1 = new User()
            {
                UserId = ObjectId.GenerateNewId(),
                UserName = "TestUser",

            };

            MapContent randomContent = ServerFunctions.GetRandomNonQuestResource().AsMapContent();
  
            var startLocation = new PlusCode("8FX9XW2F+9X", 10);
            randomContent.Location = startLocation.Code;

            DatabaseMongoDBFunctions.UpdateOrDeleteContent(randomContent, startLocation.Code).Wait();
            var result =  DatabaseMongoDBFunctions.RemoveContentAndAddToPlayer(user1.UserId, randomContent.MapId).Result;

            Debug.WriteLine("4");

            Assert.IsTrue(result);
            var userInventory =  DatabaseMongoDBFunctions.RequestInventory(user1.UserId, user1.UserId).Result;
            Assert.IsTrue(userInventory.Count == 1);

            var removeResult =  DatabaseMongoDBFunctions.RemoveContentFromInventory(user1.UserId, user1.UserId, randomContent.ToResourceDictionary()).Result;
            Assert.IsTrue(removeResult);

            userInventory =  DatabaseMongoDBFunctions.RequestInventory(user1.UserId, user1.UserId).Result;
            Assert.IsTrue(userInventory.Values.All(v => v == 0));
        }
        [Test]
        public void CanCreateUserLoginAndLogOut()
        {
            User user = new User();
            user.UserName = "test";
          
            var userDB =  DatabaseMongoDBFunctions.InsertUser(user).Result;
            var findUser =  DatabaseMongoDBFunctions.FindUserInDatabase("test").Result;
            Assert.IsTrue(findUser != null);
            var logOn =  DatabaseMongoDBFunctions.UpdateUserOnlineState(findUser.UserId.ToByteArray(), true).Result;
            Assert.IsTrue(logOn);
            var updatedUser =  DatabaseMongoDBFunctions.FindUserInDatabase("test").Result;
            Assert.IsTrue(updatedUser != null);
            Assert.IsTrue(updatedUser.CurrentlyOnline);

            var logOff =  DatabaseMongoDBFunctions.UpdateUserOnlineState(findUser.UserId.ToByteArray(), false).Result;
            Assert.IsTrue(logOff);
            updatedUser =  DatabaseMongoDBFunctions.FindUserInDatabase("test").Result;
            Assert.IsTrue(updatedUser != null);
            Assert.IsTrue(!updatedUser.CurrentlyOnline);
        }

    }
}

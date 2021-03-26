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
using System.Threading;

namespace DataModel.Server.Tests
{
    /// <summary>
    /// Unit Tests involving only MongoDB functions.
    /// </summary>
    [TestFixture] 
    public class MongoDBTests
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
        public void CanCreateUserLoginAndLogOut()
        {
            User user = new User() { UserName = "test" };
            var createAccount = MongoDBFunctions.InsertUser(user);
            createAccount.Wait();
            Assert.IsTrue(createAccount.Result);
            var findUser = MongoDBFunctions.FindUserInDatabase("test");
            Assert.IsTrue(findUser != null);
            IUser userresult = findUser.Result;
            var logOn = MongoDBFunctions.UpdateUserOnlineState(userresult.UserId, true);
            logOn.Wait();
            Assert.IsTrue(logOn.Result);
            var updatedUserTask = MongoDBFunctions.FindUserInDatabase("test");
            updatedUserTask.Wait();
            var updatedUser = updatedUserTask.Result;
            Assert.IsTrue(updatedUser != null);
            Assert.IsTrue(updatedUser.CurrentlyOnline);

            var logOffTask = MongoDBFunctions.UpdateUserOnlineState(userresult.UserId, false);
            logOffTask.Wait();
            var logOffResult = logOffTask.Result;
            Assert.IsTrue(logOffResult);
            updatedUserTask = MongoDBFunctions.FindUserInDatabase("test");
            updatedUserTask.Wait();
            updatedUser = updatedUserTask.Result;
            Assert.IsTrue(updatedUser != null);
            Assert.IsTrue(!updatedUser.CurrentlyOnline);
        }


        [Test]
        public void CanCreateAndTurnInQuests()
        {
            
        }


       

    }
}

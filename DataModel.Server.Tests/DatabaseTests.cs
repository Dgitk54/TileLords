using DataModel.Common;
using DataModel.Server.Model;
using NUnit.Framework;
using System.Linq;

namespace DataModel.Server.Tests
{
    [TestFixture]
    public class DatabaseTests
    {
        
        [Test]
        public void CanAddAndRemoveFromInventory()
        {
            //TODO: Implement interaction between redis cache and mongodb   
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

    }
}

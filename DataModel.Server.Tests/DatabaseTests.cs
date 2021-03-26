using DataModel.Common;
using DataModel.Server.Model;
using MongoDB.Bson;
using NUnit.Framework;
using System.Linq;

namespace DataModel.Server.Tests
{

    /// <summary>
    /// Tests where both databases are involved.
    /// </summary>
    [TestFixture]
    public class DatabaseTests
    {

        [Test]
        public void CanAddAndRemoveFromInventory()
        {
            IUser user1 = new User()
            {
                UserId = ObjectId.GenerateNewId(),
                UserName = "TestUser",

            };
            MapContent randomContent = ServerFunctions.GetRandomNonQuestResource().AsMapContent();

            var startLocation = new PlusCode("8FX9XW2F+9X", 10).PlusCodeToGPS();
            //"Spawn" resource:
            RedisDatabaseFunctions.UpsertContent(randomContent, startLocation.Lat, startLocation.Lon);


            //Make sure it spawned:
            var list = RedisDatabaseFunctions.RequestVisibleContent(startLocation.Lat, startLocation.Lon);
            Assert.IsTrue(list.Count == 1);


            //Pick it up
            var pickupTask = ServerFunctions.PickUpContentAndAddToInventory(randomContent.Id, user1.UserId);
            pickupTask.Wait();

            var result = pickupTask.Result;
            Assert.IsTrue(result);

            //Inventory should have items now
            var userInventory = MongoDBFunctions.RequestInventory(user1.UserId, user1.UserId).Result;
            Assert.IsTrue(userInventory.Count == 1);



            //Make sure Resource despawned
            list = RedisDatabaseFunctions.RequestVisibleContent(startLocation.Lat, startLocation.Lon);
            Assert.IsTrue(list.Count == 0);



            //Check removeContent function:
            var removeResult = MongoDBFunctions.RemoveContentFromInventory(user1.UserId, user1.UserId, randomContent.ToResourceDictionary()).Result;
            Assert.IsTrue(removeResult);



            //Inventory should have no values (keys remain):
            userInventory = MongoDBFunctions.RequestInventory(user1.UserId, user1.UserId).Result;
            Assert.IsTrue(userInventory.Values.All(v => v == 0));

        }


    }
}

using DataModel.Common;
using DataModel.Common.GameModel;
using DataModel.Common.Messages;
using DataModel.Server.Model;
using DataModel.Server.Services;
using LiteDB;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;

namespace DataModel.Server.Tests
{
    [TestFixture]
    public class MapServiceTests
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
        public void PushedContentIsVisible()
        {
            //Data setup
            MapContentService service = new MapContentService();

            var givenOne = new PlusCode("8FX9XW2F+9X", 10);
            var givenTwo = new PlusCode("8FX9XW2F+8X", 10);
            var givenThree = new PlusCode("8FX9XW2F+7X", 10);
            var givenFour = new PlusCode("8FX9XW2F+2X", 10);

            var responseList = new List<IMessage>();

            IUser user1 = new User()
            {
                UserId = ObjectId.NewObjectId(),
                UserName = "TestUser",

            };
            IUser user2 = new User()
            {
                UserId = ObjectId.NewObjectId(),
                UserName = "TestUser2",

            };
            Subject<PlusCode> user1location = new Subject<PlusCode>();
            Subject<PlusCode> user2location = new Subject<PlusCode>();

            //TEST: 1 player two requests:
            var disposable = service.AddMapContent(user1.AsMapContent(), user1location);
            Thread.Sleep(1000); //TODO: Test async task with proper structure.
            user1location.OnNext(givenOne);
            var result = service.GetMapUpdate(givenTwo.Code).Take(1).Wait();

            
            Assert.IsTrue(result.ContentList.Count == 1); // user sees himself
            

            user1location.OnNext(givenThree); // move player
            result = service.GetMapUpdate(givenTwo.Code).Take(1).Wait();
            Assert.IsTrue(result.ContentList.Count == 1); // user still sees himself



            


            //TEST: Another content enters:
            var disposable2 = service.AddMapContent(user2.AsMapContent(), user2location);
            user2location.OnNext(givenOne);


            result = service.GetMapUpdate(givenTwo.Code).Take(1).Wait();
            Assert.IsTrue(result.ContentList.Count == 2);
            Assert.IsTrue(result.ContentList[0].Type == ContentType.PLAYER);


            //TEST: player1 disconnects
            disposable.Dispose();
            result = service.GetMapUpdate(givenTwo.Code).Take(1).Wait();
            Assert.IsTrue(result.ContentList.Count == 1);

            

            //TEST: player2 disconnects
            disposable2.Dispose();
            result = service.GetMapUpdate(givenTwo.Code).Take(1).Wait();
            Assert.IsTrue(result.ContentList.Count == 0);
        }

    }
}

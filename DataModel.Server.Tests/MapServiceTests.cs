using DataModel.Common;
using DataModel.Common.Messages;
using DataModel.Server.Services;
using LiteDB;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive.Subjects;
using System.Text;

namespace DataModel.Server.Tests
{
    [TestFixture]
    public class MapServiceTests
    {
        [SetUp]
        public void Setup()
        {
            if (File.Exists(@"MyData.db"))
                File.Delete(@"MyData.db");
        }

        [TearDown]
        public void TearDown()
        {
            if (File.Exists(@"MyData.db"))
            {
                File.Delete(@"MyData.db");
            }
        }

        [Test]
        public void PushedContentIsVisible()
        {
            //Data setup
            MapContentService service = new MapContentService(DataBaseFunctions.AreaContentRequest, DataBaseFunctions.UpdateOrDeleteContent);
            
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

            user1location.OnNext(givenOne);

            service.GetMapUpdate(givenTwo.Code).Subscribe(v => responseList.AddRange(v.ContentList));

            user1location.OnNext(givenThree);

            
            
            Assert.IsTrue(responseList.Count == 1); //Only one visible content
            Assert.IsTrue(responseList[0] is ContentMessage);
            responseList.Clear();



            //TEST: Another content enters:
            var disposable2 = service.AddMapContent(user2.AsMapContent(), user2location);
            user2location.OnNext(givenOne);
            service.GetMapUpdate(givenTwo.Code).Subscribe(v => responseList.AddRange(v.ContentList));

            Assert.IsTrue(responseList.Count == 2); //two visible content
            Assert.IsTrue(responseList[0] is ContentMessage);
            var asContent = responseList[0] as ContentMessage;
            Assert.IsTrue(asContent.Type == ContentType.PLAYER); 
            responseList.Clear();


            //TEST: player1 disconnects
            disposable.Dispose();
            service.GetMapUpdate(givenTwo.Code).Subscribe(v => responseList.AddRange(v.ContentList));
            Assert.IsTrue(responseList.Count == 1); //one visible content
            Assert.IsTrue(responseList[0] is ContentMessage);
            asContent = responseList[0] as ContentMessage;
            Assert.IsTrue(asContent.Type == ContentType.PLAYER); 
            responseList.Clear();


            //TEST: player2 disconnects
            disposable2.Dispose();
            service.GetMapUpdate(givenTwo.Code).Subscribe(v => responseList.AddRange(v.ContentList));
            Assert.IsTrue(responseList.Count == 0); //0 visible content for request
            responseList.Clear();
        }

    }
}

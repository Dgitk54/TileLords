using DataModel.Common.Messages;
using DataModel.Server.Model;
using DataModel.Server.Services;
using NUnit.Framework;
using System;
using MessagePack;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Reactive.Concurrency;

namespace DataModel.Server.Tests
{
    //TODO: Fix broken tests
    [TestFixture]
    public class APIGatewayTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [TearDown]
        public void TearDown()
        {
        }


        //TODO: Fix test, wont run due to taskpool observers on APIGateway.
        [Test]
        public void RegisterGetsResponse()
        {
            var accountservice = new UserAccountService(LiteDBDatabaseFunctions.FindUserInDatabase, ServerFunctions.PasswordMatches);
            var mapservice = new MapContentService();
            var resourceSpawnService = new ResourceSpawnService(mapservice, LiteDBDatabaseFunctions.UpsertOrDeleteContent, new List<Func<List<MapContent>, bool>>() { ServerFunctions.Only5ResourcesInArea });
            var inventoryService = new InventoryService();
            var questService = new QuestService();
            var responses = new List<IMessage>();
            var options = MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray);
            var gateway = new APIGatewayService(accountservice, mapservice, resourceSpawnService, inventoryService, questService, ref options);
           
            gateway.GatewayResponse.ObserveOn(CurrentThreadScheduler.Instance).SubscribeOn(CurrentThreadScheduler.Instance).Subscribe(v => responses.Add(v));
            Subject<byte[]> testInput = new Subject<byte[]>();
            gateway.AttachGateway(testInput);


            IMessage tmp1 = new AccountMessage() { Name = "test1", Password = "test1", Context = MessageContext.REGISTER };
            IMessage tmp2 = new AccountMessage() { Name = "test1", Password = "test1", Context = MessageContext.LOGIN };

            byte[] registerRequest = MessagePackSerializer.Serialize(tmp1);
            byte[] loginRequest = MessagePackSerializer.Serialize(tmp2);

            //One register request:
            testInput.OnNext(registerRequest);
            Assert.IsTrue(responses.Count == 1);
            responses.Clear();

            //Second register message, expect error due to account existing:
            testInput.OnNext(registerRequest);
            Assert.IsTrue(responses.Count == 1);
            Assert.IsTrue(responses[0] is UserActionMessage);
            var casted = responses[0] as UserActionMessage;
            Assert.IsTrue(casted.MessageState == MessageState.ERROR);
            responses.Clear();


            //Log in:

            testInput.OnNext(loginRequest);
            Assert.IsTrue(responses.Count == 1);
            casted = responses[0] as UserActionMessage;
            Assert.IsTrue(casted.MessageState == MessageState.SUCCESS);
        }
    }
}

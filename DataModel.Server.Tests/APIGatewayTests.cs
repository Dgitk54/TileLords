using DataModel.Common.Messages;
using DataModel.Server.Model;
using DataModel.Server.Services;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace DataModel.Server.Tests
{
    [TestFixture]
    public class APIGatewayTests
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
        public void RegisterGetsResponse() //UNSUPPORTED FILTER!!!
        {
            var accountservice = new UserAccountService(MongoDBFunctions.FindUserInDatabase, ServerFunctions.PasswordMatches);
            var mapservice = new MapContentService(MongoDBFunctions.AreaContentAsMessageRequest, async (v, e) => await MongoDBFunctions.UpdateOrDeleteContent(v, e));
            var resourceSpawnService = new ResourceSpawnService(mapservice, async (v, e) => await MongoDBFunctions.UpdateOrDeleteContent(v, e), new List<Func<List<MapContent>, bool>>() { ServerFunctions.Only5ResourcesInArea });
            var inventoryService = new InventoryService();
            var questService = new QuestService();
            var responses = new List<IMessage>();
            var gateway = new APIGatewayService(accountservice, mapservice, resourceSpawnService, inventoryService, questService);

            gateway.GatewayResponse.Subscribe(v => responses.Add(v));
            Subject<IMessage> testInput = new Subject<IMessage>();
            //gateway.AttachGateway(testInput);
            IMessage registerRequest = new AccountMessage() { Name = "test1", Password = "test1", Context = MessageContext.REGISTER };
            IMessage loginRequest = new AccountMessage() { Name = "test1", Password = "test1", Context = MessageContext.LOGIN };

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

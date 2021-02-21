using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DataModel.Server.Services;
using NUnit.Framework;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Joins;
using DataModel.Common.Messages;
using System.Reactive.Subjects;

namespace DataModel.Server.Tests
{
    [TestFixture]
    public class APIGatewayTests
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
        public void RegisterGetsResponse()
        {
            var accountservice = new UserAccountService(DataBaseFunctions.FindUserInDatabase, ServerFunctions.PasswordMatches);
            var mapservice = new MapContentService(DataBaseFunctions.AreaContentRequest, DataBaseFunctions.UpdateOrDeleteContent);

            var responses = new List<IMsgPackMsg>();
            var gateway = new APIGatewayService(accountservice, mapservice);

            gateway.GatewayResponse.Subscribe(v => responses.Add(v));
            Subject<IMsgPackMsg> testInput = new Subject<IMsgPackMsg>();
            gateway.AttachGateway(testInput);
            IMsgPackMsg registerRequest = new AccountMessage() { Name = "test1", Password = "test1", Context = MessageContext.REGISTER };
            IMsgPackMsg loginRequest = new AccountMessage() { Name = "test1", Password = "test1", Context = MessageContext.REGISTER };

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

        [Test]
        public void ReceivesMapContent()
        {



        }
    }
}

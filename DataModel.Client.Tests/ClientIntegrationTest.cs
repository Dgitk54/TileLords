using DataModel.Client;
using DataModel.Common;
using DataModel.Server;
using NUnit.Framework;
using System.Threading.Tasks;
using System.Reactive.Linq;
using System;
using System.Threading;
using System.Reactive.Concurrency;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using DataModel.Common.Messages;

namespace ClientIntegration
{
    public class ClientIntegrationTest
    {
        private ServerInstance server;
        private Task serverRunning;

        [SetUp]
        public void StartServer()
        {
            if (File.Exists(@"MyData.db"))
                File.Delete(@"MyData.db");
            server = new ServerInstance();
            serverRunning = server.RunServerAsync();
        }

        [TearDown]
        public void TearDown()
        {
            if (File.Exists(@"MyData.db"))
            {
                File.Delete(@"MyData.db");
            }
            server.ShutDownServer().Wait();
        }


        [Test]
        public void ClientCanSend()
        {
            ClientInstance instance = new ClientInstance();

            var inbound = new List<IMessage>();
            var outbound = new List<IMessage>();
            var connectionState = new List<bool>();
            instance.OutboundTraffic.Subscribe(v => outbound.Add(v));
            instance.InboundTraffic.Subscribe(v => inbound.Add(v));
            instance.ClientConnectionState.Subscribe(v => connectionState.Add(v));
            var startedClientTask = ClientFunctions.StartClient(instance);

            var regMsg = new AccountMessage() { Name = "a", Password = "a", Context = MessageContext.REGISTER };
            var logMsg = new AccountMessage() { Name = "a", Password = "a", Context = MessageContext.LOGIN };
            var ctMsg = new ContentMessage() { Id = new byte[] { 5, 12, 3 }, Location = "dbg", Name = "dbgname", ResourceType = DataModel.Common.Messages.ResourceType.APPLE, Type = ContentType.RESSOURCE };
            instance.SendMessage(regMsg);
            Thread.Sleep(1500);
            instance.SendMessage(logMsg);
            Thread.Sleep(1000);
            instance.SendMessage(ctMsg);
            Thread.Sleep(1000);

            instance.DisconnectClient();
            startedClientTask.Wait();

            ;
            var client = ClientFunctions.StartClient(instance);
            Thread.Sleep(1500);
            ;
        }
       
    }
}
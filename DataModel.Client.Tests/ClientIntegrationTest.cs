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

            var inbound = new List<IMsgPackMsg>();
            var outbound = new List<IMsgPackMsg>();
            var connectionState = new List<bool>();
            instance.OutboundTraffic.Subscribe(v => outbound.Add(v));
            instance.InboundTraffic.Subscribe(v => inbound.Add(v));
            instance.ClientConnectionState.Subscribe(v => connectionState.Add(v));
            var startedClientTask = ClientFunctions.StartClient(instance);

            instance.SendMessage(new RegisterMessage() { Name = "a", Password = "a" });
            Thread.Sleep(500);
            instance.SendMessage(new LoginMessage() { Name = "a", Password = "a" });
            Thread.Sleep(1000);
            ;
        }
       
    }
}
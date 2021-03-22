using DataModel.Client;
using DataModel.Common.GameModel;
using DataModel.Common.Messages;
using DataModel.Server;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ClientIntegration
{
    public class ClientIntegrationTest
    {
        private ServerInstance server;
        private Task serverRunning;

        [SetUp]
        public void Setup()
        {
            LiteDBDatabaseFunctions.WipeAllDatabases();
            LiteDBDatabaseFunctions.InitializeDataBases();
            server = new ServerInstance();
            serverRunning = server.RunServerAsync();
        }

        [TearDown]
        public void TearDown()
        {
            LiteDBDatabaseFunctions.WipeAllDatabases();
            server.ShutDownServer().Wait();
        }
        [Test]
        public void ClientInstanceManagerReconnectTest()
        {

            var instance = new ClientInstanceManager();

            var inbound = new List<IMessage>();
            var connectionState = new List<bool>();
            var minimapupdate = new List<UnityMapMessage>();

            instance.InboundTraffic.Subscribe(v => inbound.Add(v));
            instance.ClientConnectionState.Subscribe(v => connectionState.Add(v));
            instance.ClientMapStream.Subscribe(v => minimapupdate.Add(v));


            instance.StartClient();
            var regMsg = new AccountMessage() { Name = "a", Password = "a", Context = MessageContext.REGISTER };
            var logMsg = new AccountMessage() { Name = "a", Password = "a", Context = MessageContext.LOGIN };
            var gpsMsg = new UserGpsMessage() { Lat = 49.000000, Lon = 50.00000 };
            var gspMsg2 = new UserGpsMessage() { Lat = 49.000050, Lon = 50.0000050 };

            var ctMsg = new ContentMessage() { Id = new byte[] { 5, 12, 3 }, Location = "dbg", Name = "dbgname", ResourceType = ResourceType.APPLE, Type = ContentType.RESOURCE };

            instance.SendMessage(regMsg);
            Thread.Sleep(300);
            instance.SendMessage(logMsg);
            Thread.Sleep(300);
            instance.SendMessage(ctMsg);
            Thread.Sleep(300);

            instance.SendMessage(gpsMsg);


            Thread.Sleep(300);
            instance.SendMessage(gspMsg2);

            Assert.IsTrue(minimapupdate.Count != 0);
            Assert.IsTrue(inbound.Count != 0);
        }

        [Test]
        public void ClientCanRegisterLoginAndRequestInventory()
        {
            ClientInstance instance = new ClientInstance();

            var inbound = new List<IMessage>();
            var outbound = new List<IMessage>();
            var connectionState = new List<bool>();
            var minimapupdate = new List<UnityMapMessage>();
            instance.OutboundTraffic.Subscribe(v => outbound.Add(v));
            instance.InboundTraffic.Subscribe(v => inbound.Add(v));
            instance.ClientConnectionState.Subscribe(v => connectionState.Add(v));
            instance.ClientMapStream.Subscribe(v => minimapupdate.Add(v));

            var startedClientTask = ClientFunctions.StartClient(instance);

            var regMsg = new AccountMessage() { Name = "a", Password = "a", Context = MessageContext.REGISTER };
            var logMsg = new AccountMessage() { Name = "a", Password = "a", Context = MessageContext.LOGIN };
            var gpsMsg = new UserGpsMessage() { Lat = 49.000000, Lon = 50.00000 };
            var gspMsg2 = new UserGpsMessage() { Lat = 49.000050, Lon = 50.0000050 };

            instance.SendMessage(logMsg);
            instance.SendMessage(regMsg);
            Thread.Sleep(1500);
            inbound.Clear();
            instance.SendMessage(logMsg);
            Thread.Sleep(1000);
            var response = inbound[0];
            Debug.Assert(response != null);
            Debug.Assert(response is UserActionMessage);
            var accountid = (response as UserActionMessage).AdditionalInfo;
            var requestInventory = new InventoryContentMessage() { InventoryOwner = accountid, Type = MessageType.REQUEST };
            instance.SendMessage(requestInventory);

            Thread.Sleep(1000);
            instance.SendMessage(requestInventory);

            Thread.Sleep(1000);
            instance.SendMessage(requestInventory);
            Thread.Sleep(1000);
            //instance.SendMessage(gpsMsg);
            //instance.SendMessage(gspMsg2);
            instance.DisconnectClient();
            startedClientTask.Wait();
            var client = ClientFunctions.StartClient(instance);
            Thread.Sleep(1500);
        }

    }
}
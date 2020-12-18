using DataModel.Common;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Reactive.Concurrency;
using Newtonsoft.Json;

namespace DataModel.Client.Tests
{
    public class DataSourceEventTests
    {
        static async Task<T> GetsEvent<T>(ClientInstance instance, int timeOutInSeconds) where T : IEvent
        {
            var observeOn = Scheduler.CurrentThread;
            var received = Task.Run(() =>
            {
                var result = instance.EventBus.GetEventStream<T>().Take(1).Timeout(DateTime.Now.AddSeconds(timeOutInSeconds)).ObserveOn(observeOn).Wait();
                return result;
            });
            Thread.Sleep(200);
            await received;

            return received.Result;

        }
        [Test]
        public void HandlerCanDecodeTileContentEvent()
        {
            string text = System.IO.File.ReadAllText("TestJson.txt");
            var testBus = new ClientEventBus();



            var debug = testBus.GetEventStream<DataSourceEvent>()
                                     .ParseOnlyValidUsingErrorHandler<ServerTileContentEvent>(ClientFunctions.PrintConsoleErrorHandler);

            int count = 0;
            debug.Subscribe(v => 
            {
                count++;
                ;
                Assert.IsTrue(v.VisibleContent != null);
            });
            testBus.Publish(new DataSourceEvent(text));
            Assert.IsTrue(count == 1);

        }


        [Test]
        public void UsingActualReceivedDataTileContentIsDecodable()
        {

            string text = System.IO.File.ReadAllText("ServerTileContentEvent.txt");
            var testBus = new ClientEventBus();
            int timeout = 5;

            var debug = testBus.GetEventStream<DataSourceEvent>()
                                     .ParseOnlyValidUsingErrorHandler<ServerTileContentEvent>(ClientFunctions.PrintConsoleErrorHandler);



            int count = 0;
            debug.Subscribe(v =>
            {
                count++;
                var forDebug = v;
                Assert.IsTrue(v.VisibleContent != null);
                ;
            });
            
            testBus.Publish(new DataSourceEvent(text));
            Assert.IsTrue(count == 1);

        }


        [Test]
        public void CanDeserializeDictionary()
        {
            string text = System.IO.File.ReadAllText("ServerTileContentEvent.txt");

            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                NullValueHandling = NullValueHandling.Ignore
            };
            
            var content = JsonConvert.DeserializeObject<ServerTileContentEvent>(text, settings);

            ;
        }
    }
}

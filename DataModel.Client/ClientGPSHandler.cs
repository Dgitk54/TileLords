using DataModel.Common;
using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using System.Reactive.Linq;
namespace DataModel.Client
{

    /// <summary>
    /// Responsible for intercepting GPS change, (possibly) filtering it, and publishing the change as a DataSinkEvent
    /// </summary>
    public class ClientGPSHandler
    {
        readonly IMessageBus eventBus;
        readonly JsonSerializerSettings settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All
        };
        public ClientGPSHandler(IMessageBus bus)
        {
            eventBus = bus;
        }

        IObservable<DataSinkEvent> AsDataSink(IObservable<UserGpsEvent> observable) => from e in observable
                                                                                       select new DataSinkEvent(JsonConvert.SerializeObject(e, typeof(UserGpsEvent), settings));
        public IDisposable AttachToBus() => AsDataSink(eventBus.GetEventStream<UserGpsEvent>())
            .Subscribe(v => eventBus.Publish(v));




    }
}

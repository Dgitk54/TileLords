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
        readonly IEventBus eventBus;
        public ClientGPSHandler(IEventBus bus)
        {
            eventBus = bus;
        }

        IObservable<DataSinkEvent> AsDataSink(IObservable<UserGpsChangedEvent> observable) => from e in observable
                                                                                                select new DataSinkEvent(JsonConvert.SerializeObject(e));
        public IDisposable AttachToBus() => AsDataSink(eventBus.GetEventStream<UserGpsChangedEvent>())
            .Subscribe(v => eventBus.Publish<DataSinkEvent>(v));
           
        
            
        
    }
}

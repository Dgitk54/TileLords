using DataModel.Common;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;

namespace DataModel.Server
{
    public class ClientEventBus: IEventBus
    {

        private readonly Subject<IEvent> subject = new Subject<IEvent>();

        public IObservable<T> GetEventStream<T>() where T : IEvent
        {
            return subject.OfType<T>();
        }

        public void Publish<T>(T @event) where T : IEvent
        {
            subject.OnNext(@event);
        }
    }
}

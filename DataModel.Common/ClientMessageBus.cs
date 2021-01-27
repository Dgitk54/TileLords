using DataModel.Common;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;

namespace DataModel.Common
{
    public class ClientMessageBus: IMessageBus
    {

        private readonly Subject<IMessage> subject = new Subject<IMessage>();

        public IObservable<T> GetEventStream<T>() where T : IMessage
        {
            return subject.OfType<T>();
        }

        public void Publish<T>(T @event) where T : IMessage
        {
            subject.OnNext(@event);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.Common
{
    public interface IEventBus
    {
        IObservable<T> GetEventStream<T>() where T : IEvent;

        void Publish<T>(T @event) where T : IEvent;
    }
}

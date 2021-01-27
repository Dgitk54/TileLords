using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.Common
{
    public interface IMessageBus
    {
        IObservable<T> GetEventStream<T>() where T : IMessage;

        void Publish<T>(T @event) where T : IMessage;
    }
}

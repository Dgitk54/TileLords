using DataModel.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Reactive.Linq;

namespace DataModel.Client
{
    class ClientActionErrorHandler
    {

        readonly IMessageBus eventBus;


        public ClientActionErrorHandler(IMessageBus bus)
        {
            this.eventBus = bus;

        }

        public IDisposable AttachToBus()
        {
            var onlyValid = eventBus.GetEventStream<DataSourceEvent>()
                                    .ParseOnlyValidUsingErrorHandler<UserActionErrorEvent>(ClientFunctions.PrintConsoleErrorHandler);

            var sanityChecked = from e in onlyValid
                                where e.UserAction != default
                                select e;

            return sanityChecked.Subscribe(v => eventBus.Publish(v));

        }
    }
}

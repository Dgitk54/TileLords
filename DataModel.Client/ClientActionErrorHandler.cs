using DataModel.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.Client
{
    class ClientActionErrorHandler
    {

        readonly IEventBus eventBus;


        public ClientActionErrorHandler(IEventBus bus)
        {
            this.eventBus = bus;

        }

        public IDisposable AttachToBus()
        {
            var onlyValid = eventBus.GetEventStream<DataSourceEvent>()
                                    .ParseOnlyValidUsingErrorHandler<UserActionErrorEvent>(ClientFunctions.PrintConsoleErrorHandler);

            return onlyValid.Subscribe(v => eventBus.Publish(v));

        }
    }
}

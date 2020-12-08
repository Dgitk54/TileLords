using DataModel.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.Client
{
    public class ClientActionSuccessHandler
    {
        readonly IEventBus eventBus;


        public ClientActionSuccessHandler(IEventBus bus)
        {
            eventBus = bus;
        }

        public IDisposable AttachToBus()
        {
            var onlyValid = eventBus.GetEventStream<DataSourceEvent>()
                                    .ParseOnlyValidUsingErrorHandler<UserActionSuccessEvent>(ClientFunctions.PrintConsoleErrorHandler);

            return onlyValid.Subscribe(v => eventBus.Publish(v));

        }
    }
}

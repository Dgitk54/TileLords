using DataModel.Common;
using Google.OpenLocationCode;
using LiteDB;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;

namespace DataModel.Server
{
    public class ClientAccountHandler 
    {
        readonly IEventBus eventBus;
        readonly ILiteDatabase dataBase;
        public ClientAccountHandler(IEventBus clientBus, ILiteDatabase dataBase)
        {
            eventBus = clientBus;
            this.dataBase = dataBase;
        }

        public IDisposable AttachToBus()
        {
            var registerAttempts = ServerFunctions.ParseOnlyValidUsingErrorHandler<UserRegisterEvent>(eventBus.GetEventStream<DataSourceEvent>(), ServerFunctions.PrintConsoleErrorHandler);
            var loginAttempts = ServerFunctions.ParseOnlyValidUsingErrorHandler<UserLoginEvent>(eventBus.GetEventStream<DataSourceEvent>(), ServerFunctions.PrintConsoleErrorHandler);

            


            return null;


        }


        IObservable<int> loginAttempts(IObservable<UserLoginEvent> events)
        {
            return null;
        }
    }
}

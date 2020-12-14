using DataModel.Common;
using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Joins;
using Google.OpenLocationCode;
using DotNetty.Transport.Channels;
using System.Diagnostics;
using DotNetty.Buffers;
using LiteDB;
using Newtonsoft.Json.Serialization;
using System.Security.Cryptography;

namespace DataModel.Server
{
    // Hash and salt from https://stackoverflow.com/questions/2138429/hash-and-salt-passwords-in-c-sharp
    public class ClientAccountRegisterHandler
    {
        readonly IEventBus eventBus;
        

        readonly JsonSerializerSettings settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All
        };
        public ClientAccountRegisterHandler(IEventBus clientBus, IEventBus serverBus)
        {
            eventBus = clientBus;
        }

        public IDisposable AttachToBus()
        {

            var registerAttempt = ServerFunctions.ParseOnlyValidUsingErrorHandler<UserRegisterEvent>(eventBus.GetEventStream<DataSourceEvent>(), ServerFunctions.PrintConsoleErrorHandler);


            
            var hasValues = from e in registerAttempt
                            where e != default
                            where !string.IsNullOrEmpty(e.Name)
                            where !string.IsNullOrEmpty(e.Password)
                            select e;
            
            var withCounter = hasValues.Scan(1, (counter, val) => counter++);
            var merged = registerAttempt.WithLatestFrom(withCounter, (RegisterEvent, RegisterCounter) => new { RegisterEvent, RegisterCounter });


            return merged.Subscribe(v =>
               {
                   if (v.RegisterEvent.CreateAccount())
                   {
                       var success = new UserActionSuccessEvent() { UserAction = 1 };
                       var obj = JsonConvert.SerializeObject(success, typeof(UserActionSuccessEvent), settings);
                       eventBus.Publish(new DataSinkEvent(obj));
                   }
                   else
                   {
                       var obj = new UserActionErrorEvent() { UserAction = 1 };
                       var serialized = JsonConvert.SerializeObject(obj, typeof(UserActionErrorEvent), settings);
                       eventBus.Publish(new DataSinkEvent(serialized));
                   }

               });
        }


       





    }
}

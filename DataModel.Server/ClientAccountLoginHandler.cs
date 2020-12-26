using DataModel.Common;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reactive.Linq;
using Newtonsoft.Json;
using DotNetty.Transport.Channels;

namespace DataModel.Server
{
    public class ClientAccountLoginHandler
    {
        readonly IEventBus eventBus;
        readonly IEventBus serverBus;
        readonly IObservable<bool> isActive;

        readonly IChannel channel;

        readonly JsonSerializerSettings settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All
        };
        public ClientAccountLoginHandler(IEventBus bus, IEventBus serverBus, IChannel channel, IObservable<bool> connectionActive)
        {
            eventBus = bus;
            this.channel = channel;
            this.serverBus = serverBus;
            isActive = connectionActive;
        }


        public IDisposable AttachToBus()
        {
            var registerAttempt = ServerFunctions.ParseOnlyValidUsingErrorHandler<UserLoginEvent>(eventBus.GetEventStream<DataSourceEvent>(), ServerFunctions.PrintConsoleErrorHandler);
            var onlyWithValues = from e in registerAttempt
                                 select e;


            return onlyWithValues.Subscribe(e =>
            {
                

                var user = DataBaseFunctions.InDataBase(e.Name);

                if(user == null)
                {

                    var obj = new UserActionErrorEvent() { UserAction = 2 };
                    var serialized = JsonConvert.SerializeObject(obj, typeof(UserActionErrorEvent), settings);
                    eventBus.Publish(new DataSinkEvent(serialized));
                }
                else
                {
                    var password = Encoding.UTF8.GetBytes(e.Password);
                    if(ServerFunctions.PasswordMatches(password, user.SaltedHash, user.Salt))
                    {
                        var obj = new UserActionSuccessEvent() { UserAction = 2 };
                        var serialized = JsonConvert.SerializeObject(obj, typeof(UserActionSuccessEvent), settings);
                        eventBus.Publish(new DataSinkEvent(serialized));

                        var player = new ObservablePlayer()
                        {
                            ClientBus = eventBus,
                            Name = user.UserName,
                            PlayerObservableLocationStream = ServerFunctions.ExtractPlusCodeLocationStream(eventBus, 10),
                            ClientChannel = channel,
                            ConnectionStatus = isActive
                        };

                        serverBus.Publish(new PlayerLoggedInEvent() { Player = player });
                    }
                    else
                    {
                        var obj = new UserActionErrorEvent() { UserAction = 2 };
                        var serialized = JsonConvert.SerializeObject(obj, typeof(UserActionErrorEvent), settings);
                        eventBus.Publish(new DataSinkEvent(serialized));

                    }
                }






            });
                                 
        }





        


    }
}

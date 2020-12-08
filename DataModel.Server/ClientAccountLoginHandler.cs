using DataModel.Common;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reactive.Linq;
using Newtonsoft.Json;

namespace DataModel.Server
{
    public class ClientAccountLoginHandler
    {
        readonly IEventBus eventBus;
        readonly ILiteDatabase dataBase;
        public ClientAccountLoginHandler(IEventBus bus, ILiteDatabase database)
        {
            eventBus = bus;
            dataBase = database;
        }


        public IDisposable AttachToBus()
        {
            var registerAttempt = ServerFunctions.ParseOnlyValidUsingErrorHandler<UserLoginEvent>(eventBus.GetEventStream<DataSourceEvent>(), ServerFunctions.PrintConsoleErrorHandler);
            var onlyWithValues = from e in registerAttempt
                                 where e != default
                                 where string.IsNullOrEmpty(e.Name)
                                 where string.IsNullOrEmpty(e.Password)
                                 select e;


            return onlyWithValues.Subscribe(e =>
            {
                Console.WriteLine("Firing LOGINHANDLER!");
                var col = dataBase.GetCollection<User>("users");
               
                col.EnsureIndex(v => v.AccountCreated);
                col.EnsureIndex(v => v.Inventory);
                col.EnsureIndex(v => v.LastOnline);
                col.EnsureIndex(v => v.LastPostion);
                col.EnsureIndex(v => v.UserName);
                col.EnsureIndex(v => v.Salt);
                col.EnsureIndex(v => v.SaltedHash);

                var user = InDataBase(e.Name, col);

                if(user == null)
                {
                    eventBus.Publish(new DataSinkEvent(JsonConvert.SerializeObject(new UserLoginEventError() { ErrorMessage = "Account does not exist or password is wrong." })));

                }
                else
                {
                    var password = Encoding.UTF8.GetBytes(e.Password);
                    if(ServerFunctions.PasswordMatches(password, user.SaltedHash, user.Salt))
                    {
                        var success = new UserActionSuccessEvent() { UserAction = 2 };
                        eventBus.Publish(new DataSinkEvent(JsonConvert.SerializeObject(success)));

                        //TODO:Add handlers for GPS + Gamelogic here?
                    }
                    else
                    {
                        eventBus.Publish(new DataSinkEvent(JsonConvert.SerializeObject(new UserLoginEventError() { ErrorMessage = "Account does not exist or password is wrong." })));

                    }
                }






            });
                                 
        }





        static User InDataBase(string name, ILiteCollection<User> col)
           => col.Find(v => v.UserName == name).First();


    }
}

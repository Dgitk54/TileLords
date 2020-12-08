﻿using DataModel.Common;
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
        readonly JsonSerializerSettings settings;
        public ClientAccountLoginHandler(IEventBus bus, ILiteDatabase database)
        {
            eventBus = bus;
            dataBase = database;
            settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
        }


        public IDisposable AttachToBus()
        {
            var registerAttempt = ServerFunctions.ParseOnlyValidUsingErrorHandler<UserLoginEvent>(eventBus.GetEventStream<DataSourceEvent>(), ServerFunctions.PrintConsoleErrorHandler);
            var onlyWithValues = from e in registerAttempt
                                 select e;


            return onlyWithValues.Subscribe(e =>
            {
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

                    var obj = new UserLoginEventError() { ErrorMessage = "Account does not exist or password is wrong." };
                    var serialized = JsonConvert.SerializeObject(obj, typeof(UserLoginEventError), settings);
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

                        //TODO:Add handlers for GPS + Gamelogic here?
                    }
                    else
                    {
                        var obj = new UserLoginEventError() { ErrorMessage = "Account does not exist or password is wrong." };
                        var serialized = JsonConvert.SerializeObject(obj, typeof(UserLoginEventError), settings);
                        eventBus.Publish(new DataSinkEvent(serialized));
                       
                    }
                }






            });
                                 
        }





        static User InDataBase(string name, ILiteCollection<User> col)
        {
            if(col.Find(v => v.UserName == name).Any())
                return col.Find(v => v.UserName == name).First();
            return null;
        }


    }
}

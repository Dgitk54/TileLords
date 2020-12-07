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
        readonly ILiteDatabase dataBase;
        public ClientAccountRegisterHandler(IEventBus clientBus, ILiteDatabase dataBase, IEventBus serverBus)
        {
            eventBus = clientBus;
            this.dataBase = dataBase;
           

        }

        public IDisposable AttachToBus()
        {

            var registerAttempt = ServerFunctions.ParseOnlyValidUsingErrorHandler<UserRegisterEvent>(eventBus.GetEventStream<DataSourceEvent>(), ServerFunctions.PrintConsoleErrorHandler);
            var withCounter = registerAttempt.Scan(1, (counter, val) => counter++);
            var merged = registerAttempt.WithLatestFrom(withCounter, (RegisterEvent, RegisterCounter) => new { RegisterEvent, RegisterCounter });


            return merged.Subscribe(v =>
               {
                   if (CreateAccount(v.RegisterEvent))
                   {
                       var success = new UserActionSuccessEvent() { UserAction = 1 };
                       eventBus.Publish(new DataSinkEvent(JsonConvert.SerializeObject(success)));

                   }
                   else
                   {
                       eventBus.Publish(new DataSinkEvent(JsonConvert.SerializeObject(new UserRegisterEventError() { ErrorMessage = "Could not create account, username taken?" })));
                   }

               });
        }


        bool CreateAccount(UserRegisterEvent user)
        {

            var col = dataBase.GetCollection<User>("users");
            col.EnsureIndex(v => v.AccountCreated);
            col.EnsureIndex(v => v.Inventory);
            col.EnsureIndex(v => v.LastOnline);
            col.EnsureIndex(v => v.LastPostion);
            col.EnsureIndex(v => v.UserName);
            col.EnsureIndex(v => v.Salt);
            col.EnsureIndex(v => v.SaltedHash);

            if (NameTaken(user.Name, col))
                return false;

            byte[] salt;
            new RNGCryptoServiceProvider().GetBytes(salt = new byte[16]);

            var password = ServerFunctions.Hash(user.Password, salt);

            var userForDb = new User
            {
                AccountCreated = DateTime.Now,
                Salt = salt,
                SaltedHash = password,
                UserName = user.Name
            };

            col.Insert(userForDb);
            return true;

        }


        static bool NameTaken(string name, ILiteCollection<User> col)
            => col.Find(v => v.UserName == name).Any();





    }
}

using DataModel.Server.Model;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DataModel.Server.Services
{
    /// <summary>
    /// Service responsible for Accountmanagement
    /// </summary>
    public class UserAccountService
    {

        readonly Func<byte[], byte[], byte[], bool> passwordMatcher;

        public UserAccountService(Func<byte[], byte[], byte[], bool> passwordMatchAlgorithm)
        {
            passwordMatcher = passwordMatchAlgorithm;
        }


        public IObservable<IUser> LoginUser(string name, string password)
        {
            return Observable.Create<IUser>(async v =>
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                var user = await MongoDBFunctions.FindUserInDatabase(name);

                if (user == null)
                {
                    v.OnError(new Exception("Unkown User"));
                    return Disposable.Empty;
                }

                if (user.CurrentlyOnline)
                {
                    v.OnError(new Exception("User is online!"));
                    return Disposable.Empty;
                }
                var pass = Encoding.UTF8.GetBytes(password);
                var matcher = Task.Run(() => passwordMatcher(pass, user.SaltedHash, user.Salt));
                var result = await matcher;
                if (result)
                {
                    stopwatch.Stop();
                    Console.WriteLine("LOG IN: Elapsed Time is {0} ms" + name, stopwatch.ElapsedMilliseconds);
                    v.OnNext(user);
                    //  var updateUser = Task.Run(() => DataBaseFunctions.UpdateUserOnlineState(user.UserId.ToByteArray(), true));
                    //  await updateUser;
                    v.OnCompleted();
                    return Disposable.Empty;
                }
                else
                {
                    v.OnError(new Exception("Password does not match"));
                    return Disposable.Empty;
                }
            });
        }

        public IObservable<bool> RegisterUser(string name, string password)
        {
            return Observable.Defer(() => Observable.Start(() =>
                   {
                       byte[] salt;
                       new RNGCryptoServiceProvider().GetBytes(salt = new byte[16]);
                       return salt;
                   }))
                   .Select(salt =>
                   {
                       var obs = Observable.Defer(() => Observable.Start(() => ServerFunctions.Hash(password, salt))).Select(e => new User()
                       {
                           AccountCreated = DateTime.Now,
                           Salt = salt,
                           SaltedHash = e,
                           UserName = name,
                           CurrentlyOnline = false,
                           UserId = ObjectId.GenerateNewId()
                       }) ;

                       return obs;

                   })
                   .Switch()
                   .Select(v => MongoDBFunctions.InsertUser(v))
                   .Switch();
        }
        public IDisposable LogOffUseronDispose(IUser user)
        {
            return Disposable.Create( () => {  MongoDBFunctions.UpdateUserOnlineState(user.UserId, false); });
        }

        public IObservable<bool> LogoutUser(string name)
        {
            return Observable.FromAsync(v => MongoDBFunctions.UpdateUserOnlineState(name, false));
        }

    }
}


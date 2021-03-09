using DataModel.Server.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;

namespace DataModel.Server.Services
{
    /// <summary>
    /// Service responsible for Accountmanagement
    /// </summary>
    public class UserAccountService
    {

        readonly Func<string, User> userNameLookup;
        readonly Func<byte[], byte[], byte[], bool> passwordMatcher;

        public UserAccountService(Func<string, User> userDatabaseLookup, Func<byte[], byte[], byte[], bool> passwordMatchAlgorithm)
        {
            passwordMatcher = passwordMatchAlgorithm;
            userNameLookup = userDatabaseLookup;
        }

        
        public IObservable<IUser> LoginUser(string name, string password)
        {
            return Observable.Create<IUser>(v =>
            {
                var user = userNameLookup(name);
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
                if (passwordMatcher(pass, user.SaltedHash, user.Salt))
                {
                    DataBaseFunctions.UpdateUserOnlineState(user.UserId.ToByteArray(), true);
                    v.OnNext(user);
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
            return Observable.Create<bool>(v =>
            {
                var user = userNameLookup(name);
                if (user != null)
                    v.OnError(new Exception("Username taken"));

                var result = DataBaseFunctions.CreateAccount(name, password);
                v.OnNext(result);
                v.OnCompleted();
                return Disposable.Empty;
            });
        }
        public IDisposable LogOffUseronDispose(IUser user)
        {
            return Disposable.Create(() => DataBaseFunctions.UpdateUserOnlineState(user.UserId, false));
        }
        
        public IObservable<bool> LogoutUser(string name)
        {
            return Observable.Create<bool>(v =>
            {
                var result = DataBaseFunctions.UpdateUserOnlineState(name, false);
                v.OnNext(result);
                v.OnCompleted();
                return Disposable.Empty;
            });
        }

    }
}


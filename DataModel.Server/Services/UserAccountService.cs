using DataModel.Common;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reactive.Linq;
using DotNetty.Transport.Channels;
using System.Reactive.Subjects;
using System.Reactive.Disposables;
using System.Collections.Concurrent;
using DataModel.Server.Model;

namespace DataModel.Server.Services
{
    /// <summary>
    /// Service responsible for Accountmanagement
    /// </summary>
    public class UserAccountService
    {

        readonly BehaviorSubject<List<IUser>> onlineUsers = new BehaviorSubject<List<IUser>>(new List<IUser>());
        readonly ISubject<List<IUser>> synchronizedOnlineUsers;
        readonly Func<string, User> userNameLookup;
        readonly Func<byte[], byte[], byte[], bool> passwordMatcher;
        
        public UserAccountService(Func<string, User> userDatabaseLookup, Func<byte[], byte[], byte[], bool> passwordMatchAlgorithm )
        {
            passwordMatcher = passwordMatchAlgorithm;
            userNameLookup = userDatabaseLookup;
            synchronizedOnlineUsers = Subject.Synchronize(onlineUsers);
        }

        public IObservable<IList<IUser>> OnlineUsers => synchronizedOnlineUsers.AsObservable();

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

                var sameNameOnlineCount = onlineUsers.Value.Where(u => u.UserName.Equals(name)).Count();
                if (sameNameOnlineCount != 0)
                {
                    v.OnError(new Exception("User already online"));
                    return Disposable.Empty;
                }

                var pass = Encoding.UTF8.GetBytes(password);
                if(passwordMatcher(pass, user.SaltedHash, user.Salt))
                {
                    var updateOnline = onlineUsers.Value;
                    updateOnline.Add(user);
                    synchronizedOnlineUsers.OnNext(updateOnline);
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

        public IObservable<bool> LogoutUser(IUser user)
        {
            return Observable.Create<bool>(v =>
            {
                var val = onlineUsers.Value;
                if (!val.Contains(user))
                    v.OnError(new Exception("Could not log out user with id" + Convert.ToString(user.UserId)));

                val.Remove(user);
                synchronizedOnlineUsers.OnNext(val);
                v.OnNext(true);
                v.OnCompleted();
                return Disposable.Empty;
            });
        }

    }
}


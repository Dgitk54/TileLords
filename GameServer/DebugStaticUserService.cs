using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;

namespace GameServer
{
    /// <summary>
    /// Test UserService to measure performance of DotNetty + RX
    /// </summary>
    public static class DebugStaticUserService
    {
        public static IObservable<IUser> LoginUser(string name, string password)
        {
            return Observable.Return(new User() { UserId = Guid.NewGuid() });
        }

        public static IObservable<bool> RegisterUser(string name, string password)
        {
            return Observable.Return(true);
        }
    }
}

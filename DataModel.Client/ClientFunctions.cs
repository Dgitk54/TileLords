using DataModel.Common;
using DataModel.Common.Messages;
using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DataModel.Client
{
    /// <summary>
    /// Class for functions shared between multiple handlers.
    /// </summary>
    public static class ClientFunctions
    {
        public static Task StartClient(ClientInstance instance, string ip, int port)
        {
            var waitForConnection = Task.Run(() =>
            {
                var result = instance.ClientConnectionState.Do(v => Console.WriteLine(v)).Where(v => v).Take(1)
                            .Timeout(DateTime.Now.AddSeconds(5)).Wait();
                return result;
            });
            Thread.Sleep(300);
            var run = Task.Run(() => instance.RunClientAsyncWithIP(ip, port));
            waitForConnection.Wait();
            return run;
        }
        public static Task StartClient(ClientInstance instance)
        {
            var waitForConnection = Task.Run(() =>
            {
                var result = instance.ClientConnectionState.Do(v => Console.WriteLine(v)).Where(v => v).Take(1)
                            .Timeout(DateTime.Now.AddSeconds(5)).Wait();
                return result;
            });
            Thread.Sleep(300);
            var run = Task.Run(() => instance.RunClientAsyncWithIP());
            waitForConnection.Wait();
            return run;
        }
        public static void SendGpsPath(ClientInstanceManager instance, CancellationToken ct, List<GPS> gps, int sleeptime)
        {
            int i = 0;
            do
            {
                var gpsNode = gps[i % gps.Count];
                instance.SendMessage(new UserGpsMessage() { Lat = gpsNode.Lat, Lon = gpsNode.Lon });
                Thread.Sleep(sleeptime);
                i++;
                if (ct.IsCancellationRequested)
                    break;

            } while (!ct.IsCancellationRequested);
        }


        public static async Task<Tout> GetsEvent<Tout, Tin>(ClientInstanceManager instance, Tin input, int timeOutInSeconds) where Tout : IMessage where Tin : IMessage
        {
            var observeOn = Scheduler.CurrentThread;
            var received = Task.Run(() =>
            {
                var result = instance.InboundTraffic.OfType<Tout>().Take(1).Timeout(DateTime.Now.AddSeconds(timeOutInSeconds)).ObserveOn(observeOn).Wait();
                return result;
            });
            Thread.Sleep(200);
            var publish = Task.Run(() => instance.SendMessage(input));
            await received;
            await publish;
            return received.Result;
        }

        public static void TryRegisterAndLogInInfiniteAttempts(ClientInstanceManager instance, string name, string password)
        {
            
            
            var tryRegister = GetsEvent<UserActionMessage, AccountMessage>(instance, new AccountMessage() { Name = name, Password = password, Context = MessageContext.REGISTER }, 5);
            tryRegister.Wait();
            var registerResponse = tryRegister.Result;
            tryRegister.Dispose();
            if (!(registerResponse.MessageState == MessageState.SUCCESS && registerResponse.MessageContext == MessageContext.REGISTER))
            {
            }
            //Log in after register:
            Thread.Sleep(300);
            UserActionMessage loginResponse = null;
            while(loginResponse == null)
            {
                try
                {
                    var tryLogin = GetsEvent<UserActionMessage, AccountMessage>(instance, new AccountMessage() { Name = name, Password = password, Context = MessageContext.LOGIN }, 5);
                    tryLogin.Wait();
                    loginResponse = tryLogin.Result;
                    tryLogin.Dispose();
                } catch(Exception )
                {
                    Console.WriteLine("Log in timed out, retrying with!" + name);
                }
                
            }
            
            
            if (!(loginResponse.MessageState == MessageState.SUCCESS && loginResponse.MessageContext == MessageContext.LOGIN))
            {
                throw new Exception("Error logging in after registering");
            }

        }

        public static void LoginOrRegisterAndLogin(ClientInstanceManager instance, string name, string password)
        {
            var tryLogin = GetsEvent<UserActionMessage, AccountMessage>(instance, new AccountMessage() { Name = name, Password = password, Context = MessageContext.LOGIN }, 5);
            tryLogin.Wait();
            var loginResponse = tryLogin.Result;
            tryLogin.Dispose();
            if (loginResponse.MessageContext == MessageContext.LOGIN && loginResponse.MessageState == MessageState.ERROR)
            {
                //Login Failed, try register with name password
                var tryRegister = GetsEvent<UserActionMessage, AccountMessage>(instance, new AccountMessage() { Name = name, Password = password, Context = MessageContext.REGISTER }, 5);
                tryRegister.Wait();
                var registerResponse = tryRegister.Result;
                tryRegister.Dispose();
                if (!(registerResponse.MessageState == MessageState.SUCCESS && registerResponse.MessageContext == MessageContext.REGISTER))
                {
                    throw new Exception("Error logging in and registering");
                }
                //Log in after register:
                Thread.Sleep(300);
                tryLogin = GetsEvent<UserActionMessage, AccountMessage>(instance, new AccountMessage() { Name = name, Password = password, Context = MessageContext.LOGIN }, 5);
                tryLogin.Wait();
                loginResponse = tryLogin.Result;
                tryLogin.Dispose();
                if (!(loginResponse.MessageState == MessageState.SUCCESS && loginResponse.MessageContext == MessageContext.LOGIN))
                {
                    throw new Exception("Error logging in after registering");
                }
            }

        }

    }
}

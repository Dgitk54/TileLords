using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using DataModel.Client;
using DataModel.Common;
using DataModel.Common.Messages;

namespace ClientMain
{
    class Program
    {
        static void Main(string[] args)
        {
            var token = new CancellationTokenSource();
            for (; ; )
            {
                Console.WriteLine("Enter your RegisterUsername:");
                var name = Console.ReadLine();
                if (string.IsNullOrEmpty(name))
                    continue;
                Console.WriteLine("Enter your RegisterPassword");
                var password = Console.ReadLine();
                if (string.IsNullOrEmpty(password))
                    continue;

                Console.WriteLine("LocationInt a: GPS(49.000000, 7.900150)        b: GPS(49.000000, 7.900000)          c : GPS(49.000000, 7.900300) ");
                var locationChar = Console.ReadLine();
                if (string.IsNullOrEmpty(password))
                    continue;

                var debugJumpLista = new List<GPS>() { new GPS(49.000000, 7.900300), new GPS(49.000000, 7.900000) };
                var debugJumpListb = new List<GPS>() { new GPS(49.000300, 7.900000), new GPS(49.000600, 7.900000) };
                var debugJumpListc = new List<GPS>() { new GPS(49.000300, 7.900300), new GPS(49.000300, 7.900300) };

                switch (locationChar[0])
                {
                    case 'a':
                              Task.Run(() => DebugLoginAndRunAroundClient(name, password, debugJumpLista, token.Token));
                        break;
                    case 'b':
                              Task.Run(() => DebugLoginAndRunAroundClient(name, password, debugJumpListb, token.Token));
                        break;
                    case 'c':
                              Task.Run(() => DebugLoginAndRunAroundClient(name, password, debugJumpListc, token.Token));
                        break;
                }
            }
        }
        
        static async Task<Tout> GetsEvent<Tout, Tin>(ClientInstance instance, Tin input, int timeOutInSeconds) where Tout : IMsgPackMsg where Tin : IMsgPackMsg
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

        static void DebugLoginAndRunAroundClient(string name, string password, List<GPS> path, CancellationToken cancellationToken)
        {
            var instance = new ClientInstance();
            var result = ClientFunctions.StartClient(instance);
            //result.Wait();
            ;
            var tokenSrc = new CancellationTokenSource();
            instance.SendLoginRequest("test1", "test2");
            //Try to log in, create account if cant log in:
            var tryLogin = GetsEvent<UserActionMessage, LoginMessage>(instance, new LoginMessage() { Name = name, Password = password }, 5);
            tryLogin.Wait();
            var loginResponse = tryLogin.Result;
            tryLogin.Dispose();
            if (loginResponse.MessageContext == MessageContext.LOGIN && loginResponse.MessageState == MessageState.ERROR)
            {
                //Login Failed, try register with name password
                var tryRegister = GetsEvent<UserActionMessage, RegisterMessage>(instance, new RegisterMessage() { Name = name, Password = password }, 5);
                tryRegister.Wait();
                var registerResponse = tryRegister.Result;
                tryRegister.Dispose();
                if (!(registerResponse.MessageState == MessageState.SUCCESS && registerResponse.MessageContext == MessageContext.REGISTER))
                {
                    throw new Exception("Error logging in and registering");
                }
                //Log in after register:
                tryLogin = GetsEvent<UserActionMessage, LoginMessage>(instance, new LoginMessage() { Name = name, Password = password }, 5);
                tryLogin.Wait();
                loginResponse = tryLogin.Result;
                tryLogin.Dispose();
                if (!(registerResponse.MessageState == MessageState.SUCCESS && registerResponse.MessageContext == MessageContext.LOGIN))
                {
                    throw new Exception("Error logging in after registering");
                }
            }
            ;
            Thread.Sleep(3000);
            var sendPath = Task.Run(() => ClientFunctions.SendGpsPath(instance, tokenSrc.Token, path, 4000));
            do
            {
                if (cancellationToken.IsCancellationRequested)
                    break;
                Thread.Sleep(1000);

            } while (!cancellationToken.IsCancellationRequested);

            tokenSrc.Cancel();
            sendPath.Wait();
            instance.DisconnectClient();
        }

    }
}

using DataModel.Common;
using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Reactive.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using MessagePack;
using DataModel.Common.Messages;
using System.Threading.Tasks;
using System.Threading;
using System.Reactive.Concurrency;

namespace DataModel.Client
{
    /// <summary>
    /// Class for functions shared between multiple handlers.
    /// </summary>
    public static class ClientFunctions
    {
        public static IDisposable EventStreamSink<T>(IObservable<T> objStream, IChannelHandlerContext context) where T : IMsgPackMsg
            => objStream.Subscribe(v =>
            {
                //var lz4Options = MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray);
                var data = MessagePackSerializer.Serialize(v);
                Console.WriteLine("PUSHING: DATA" + data.GetLength(0));
                context.WriteAndFlushAsync(Unpooled.WrappedBuffer(data));
            },
             e => Console.WriteLine("Error occured writing" + objStream),
             () => Console.WriteLine("StreamSink Write Sequence Completed"));
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
        public static void SendGpsPath(ClientInstance instance, CancellationToken ct, List<GPS> gps, int sleeptime)
        {
            int i = 0;
            do
            {
                instance.SendGps(gps[i % gps.Count]);
                Thread.Sleep(sleeptime);
                i++;
                if (ct.IsCancellationRequested)
                    break;

            } while (!ct.IsCancellationRequested);
        }


        public static async Task<Tout> GetsEvent<Tout, Tin>(ClientInstance instance, Tin input, int timeOutInSeconds) where Tout : IMsgPackMsg where Tin : IMsgPackMsg
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

        public static void LoginOrRegister(ClientInstance instance, string name, string password)
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
                tryLogin = GetsEvent<UserActionMessage, AccountMessage>(instance, new AccountMessage() { Name = name, Password = password, Context = MessageContext.LOGIN }, 5);
                tryLogin.Wait();
                loginResponse = tryLogin.Result;
                tryLogin.Dispose();
                if (!(registerResponse.MessageState == MessageState.SUCCESS && registerResponse.MessageContext == MessageContext.LOGIN))
                {
                    throw new Exception("Error logging in after registering");
                }
            }

        }

    }
}

using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Reactive.Linq;
using MessagePack;
using DataModel.Common.Messages;
using System.Reactive.Concurrency;

namespace GameServer
{
    public static class ReactiveSocketWrapper
    {
        static MessagePackSerializerOptions lz4Options = MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray);
        static readonly UserActionMessage loginFail = new UserActionMessage()
        {
            MessageContext = MessageContext.LOGIN,
            MessageInfo = MessageInfo.LOGINFAIL,
            MessageState = MessageState.ERROR,
            MessageType = MessageType.RESPONSE
        };
        static readonly UserActionMessage loginSuccess = new UserActionMessage()
        {
            MessageContext = MessageContext.LOGIN,
            MessageInfo = MessageInfo.NONE,
            MessageState = MessageState.SUCCESS,
            MessageType = MessageType.RESPONSE
        };
        static readonly UserActionMessage registerFail = new UserActionMessage()
        {
            MessageContext = MessageContext.REGISTER,
            MessageInfo = MessageInfo.NONE,
            MessageState = MessageState.ERROR,
            MessageType = MessageType.RESPONSE
        };
        static readonly UserActionMessage registerSuccess = new UserActionMessage()
        {
            MessageContext = MessageContext.REGISTER,
            MessageInfo = MessageInfo.NONE,
            MessageState = MessageState.SUCCESS,
            MessageType = MessageType.RESPONSE
        };

        static byte[] NewLineDelimiter = new byte[] { (byte)'\n' };
        public static IObservable<ReadOnlyMemory<byte>> AttachGatewayNoTask(IObservable<byte[]> inbound)
        {
            return inbound.Select(v => MessagePackSerializer.Deserialize<IMessage>(v, lz4Options))
                          .Select(v =>
                          {
                              var observable = Observable.Return(v);
                              var register = HandleRegisterResponse(observable);
                              var login = LoginRequestsResponse(observable);
                              var consolePrinter = ConsolePrinter(observable);
                              return Observable.Concat(register, login, consolePrinter);
                          }).SelectMany(v => v)
                          .Select(v =>
                          {
                              var data = MessagePackSerializer.Serialize(v, lz4Options);

                              var result = data.Concat(NewLineDelimiter).ToArray();
                              return new ReadOnlyMemory<byte>(result);
                          });
        }

        public static IObservable<ReadOnlyMemory<byte>> AttachGateWay(IObservable<byte[]> inbound)
        {
            return inbound.ObserveOn(TaskPoolScheduler.Default)
                   //Deserialization:
                   .Select(v => Observable.Defer(() =>
                   {
                       return Observable.Start(() =>
                       {
                           return MessagePackSerializer.Deserialize<IMessage>(v, lz4Options);
                       })
                       .Catch<IMessage, Exception>(v =>
                       {
                           return Observable.Empty<IMessage>();
                       });
                   }))
                   //Handle Messages
                   .SelectMany(messages =>
                   {
                       var register = HandleRegisterResponse(messages);
                       var login = LoginRequestsResponse(messages);
                       var consolePrinter = ConsolePrinter(messages);
                       var userAndMore = CurrentlyLoggedInUser(messages).Where(user => user != null) //User logged in
                                                                        .SelectMany(user =>
                                                                        {
                                                                            //Handle user with messages, subscribe to map/inventory/quest services etc.
                                                                            return Observable.Concat(DoRandomStuff(messages, user), DoRandomStuff2(messages, user));
                                                                        });
                       //concat responses
                       return Observable.Concat(register, login, consolePrinter, userAndMore);
                   })
                   //serialize responses
                   .Select(v =>
                   {
                       var data = MessagePackSerializer.Serialize(v, lz4Options);
                       var result = data.Concat(NewLineDelimiter).ToArray();
                       return new ReadOnlyMemory<byte>(result);
                   });;
        }

        public static IObservable<IMessage> ConsolePrinter(IObservable<IMessage> inbound)
        {
            return inbound.Do(v => { Console.WriteLine(v.ToString()); })
                   .Select(v => new InventoryContentMessage() { MessageState = MessageState.SUCCESS, Type = MessageType.RESPONSE });
        }
        public static IObservable<IMessage> DoRandomStuff(IObservable<IMessage> inbound, IUser user)
        {
            return inbound.OfType<UserGpsMessage>().Sample(TimeSpan.FromSeconds(5)).Select(v => new InventoryContentMessage() { MessageState = MessageState.SUCCESS, Type = MessageType.RESPONSE, InventoryOwner = user.UserId });
        }

        public static IObservable<IMessage> DoRandomStuff2(IObservable<IMessage> inbound, IUser user)
        {
            return inbound.OfType<UserGpsMessage>().Sample(TimeSpan.FromSeconds(10)).Select(v => new InventoryContentMessage() { MessageState = MessageState.SUCCESS, Type = MessageType.RESPONSE, InventoryOwner = user.UserId });
        }


        public static IObservable<IMessage> HandleRegisterResponse(IObservable<IMessage> inbound)
        {
            return inbound.OfType<AccountMessage>()
                                .Where(v => v.Context == MessageContext.REGISTER)
                                .SelectMany(v =>
                                {
                                    return DebugStaticUserService.RegisterUser(v.Name, v.Password).Catch<bool, Exception>(tx => Observable.Return(false));
                                })
                                .Select(v =>
                                {
                                    if (v)
                                    {
                                        return registerSuccess;
                                    }
                                    else
                                    {
                                        return registerFail;
                                    }
                                });


        }

        public static IObservable<IMessage> LoginRequestsResponse(IObservable<IMessage> inbound)
        {
            return inbound.OfType<AccountMessage>()
                                      .Where(v => v.Context == MessageContext.LOGIN)
                                      .SelectMany(v =>
                                      {
                                          return DebugStaticUserService.LoginUser(v.Name, v.Password).Catch<IUser, Exception>(tx =>
                                          {
                                              return Observable.Empty<IUser>();
                                          });
                                      })
                                      .Select(v =>
                                      {

                                          if (v != null)
                                          {
                                              return loginSuccess;
                                          }
                                          else
                                          {
                                              return loginFail;
                                          }
                                      });

        }


        public static IObservable<IUser> CurrentlyLoggedInUser(IObservable<IMessage> inbound)
        {
            return inbound.OfType<AccountMessage>()
                                      .Where(v => v.Context == MessageContext.LOGIN)
                                      .SelectMany(v =>
                                      {
                                          return DebugStaticUserService.LoginUser(v.Name, v.Password).Catch<IUser, Exception>(tx =>
                                          {
                                              return Observable.Empty<IUser>();
                                          });
                                      });
        }

    }
}

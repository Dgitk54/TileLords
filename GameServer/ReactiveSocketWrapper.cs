using DataModel.Common.Messages;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;

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
        public static IObservable<IMessage> AttachGateWay(IObservable<byte[]> inbound)
        {
            return inbound.ObserveOn(TaskPoolScheduler.Default).Select(v => Observable.Defer(() => Observable.Start(() => MessagePackSerializer.Deserialize<IMessage>(v, lz4Options)).Catch<IMessage,Exception>(v=> { return Observable.Empty<IMessage>(); }) ))
                          .SelectMany(v =>
                          {
                              var register = HandleRegisterResponse(v);
                              var login = LoginRequestsResponse(v);
                              var consolePrinter = ConsolePrinter(v);
                              var userAndMore = CurrentlyLoggedInUser(v).Where(e => e != null).SelectMany(e => { return Observable.Concat(DoRandomStuff(v, e), DoRandomStuff2(v, e)); });

                              return Observable.Concat(register, login, consolePrinter);
                          });
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

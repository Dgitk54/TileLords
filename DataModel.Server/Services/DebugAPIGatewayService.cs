using DataModel.Common;
using DataModel.Common.GameModel;
using DataModel.Common.Messages;
using DataModel.Server.Model;
using DotNetty.Buffers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace DataModel.Server.Services
{
    /// <summary>
    /// Test APIGateway to measure performance of DotNetty + RX
    /// </summary>
    public class DebugAPIGatewayService
    {

        public static IObservable<IMessage> AttachGateWay(IObservable<IByteBuffer> inbound)
        {
            return inbound.ObserveOn(TaskPoolScheduler.Default)
                          .Select(v => Observable.Defer(() => Observable.Start(() => v.ToString(Encoding.UTF8).FromString())))
                          .SelectMany(v =>
                          {
                              var register = HandleRegisterResponse(v);
                              var login = LoginRequestsResponse(v);
                              //var userAndMore = CurrentlyLoggedInUser(v).Where(e => e != null).SelectMany(e => { return Observable.Concat(DoRandomStuff(v, e), DoRandomStuff2(v, e)); });

                              return Observable.Concat(register, login);
                          });
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
            return inbound.ObserveOn(TaskPoolScheduler.Default).OfType<AccountMessage>()
                                .Where(v => v.Context == MessageContext.REGISTER)
                                .SelectMany(v =>
                                {
                                    return DebugStaticUserService.RegisterUser(v.Name, v.Password).Catch<bool, Exception>(tx => Observable.Return(false));
                                })
                                .Select(v =>
                                {
                                    if (v)
                                    {
                                        return GatewayResponses.registerSuccess;
                                    }
                                    else
                                    {
                                        return GatewayResponses.registerFail;
                                    }
                                });


        }

        public static IObservable<IMessage> LoginRequestsResponse(IObservable<IMessage> inbound)
        {
            return inbound.ObserveOn(TaskPoolScheduler.Default).OfType<AccountMessage>()
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
                                              return GatewayResponses.loginSuccess;
                                          }
                                          else
                                          {
                                              return GatewayResponses.loginFail;
                                          }
                                      });

        }


        public static IObservable<IUser> CurrentlyLoggedInUser(IObservable<IMessage> inbound)
        {
            return inbound.ObserveOn(TaskPoolScheduler.Default).OfType<AccountMessage>()
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

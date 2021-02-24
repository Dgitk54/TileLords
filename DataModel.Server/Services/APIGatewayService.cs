using DataModel.Common.Messages;
using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Reactive;
using System.Text;
using System.Reactive.Linq;
using DataModel.Common;
using System.Diagnostics;
using System.Collections.Concurrent;
using System.Reactive.Disposables;

namespace DataModel.Server.Services
{
    public class APIGatewayService
    {
        
        readonly Subject<IMessage> responses = new Subject<IMessage>();
        readonly UserAccountService userService;
        readonly MapContentService mapService;
        readonly ResourceSpawnService resourceSpawnService;
        readonly List<IDisposable> disposables = new List<IDisposable>();

        readonly static UserActionMessage loginFail = new UserActionMessage()
        {
            MessageContext = MessageContext.LOGIN,
            MessageInfo = MessageInfo.LOGINFAIL,
            MessageState = MessageState.ERROR,
            MessageType = MessageType.RESPONSE
        };
        
        readonly static UserActionMessage loginSuccess = new UserActionMessage()
        {
            MessageContext = MessageContext.LOGIN,
            MessageInfo = MessageInfo.NONE,
            MessageState = MessageState.SUCCESS,
            MessageType = MessageType.RESPONSE
        };
        readonly static UserActionMessage registerFail = new UserActionMessage()
        {
            MessageContext = MessageContext.REGISTER,
            MessageInfo = MessageInfo.NONE,
            MessageState = MessageState.ERROR,
            MessageType = MessageType.RESPONSE
        };
        readonly static UserActionMessage registerSuccess = new UserActionMessage()
        {
            MessageContext = MessageContext.REGISTER,
            MessageInfo = MessageInfo.NONE,
            MessageState = MessageState.SUCCESS,
            MessageType = MessageType.RESPONSE
        };

        public APIGatewayService(UserAccountService userService, MapContentService mapService, ResourceSpawnService spawnService)
        {
            this.userService = userService;
            this.mapService = mapService;
            this.resourceSpawnService = spawnService;
        }
        public IObservable<IMessage> GatewayResponse => responses.AsObservable();
        public void AttachGateway(IObservable<IMessage> inboundtraffic)
        {
            disposables.Add(HandleRegister(inboundtraffic));
        
            LoggedInUser(inboundtraffic).Take(1).Subscribe(v =>
            {
                var mapServicePlayerUpdate = mapService.AddMapContent(v.AsMapContent(), LatestClientLocation(inboundtraffic));
                var mapDataRequests = Observable.Interval(TimeSpan.FromSeconds(3)).WithLatestFrom(LatestClientLocation(inboundtraffic), (time, location) => new { time, location })
                                                                                  .Select(v2 => v2.location)
                                                                                  .Select(v2 => mapService.GetMapUpdate(v2.Code))
                                                                                  .Switch()
                                                                                  .Subscribe(v2 => responses.OnNext(v2));

                resourceSpawnService.AddMovableRessourceSpawnArea(v.UserId, LatestClientLocation(inboundtraffic));
                                    
                
                
                disposables.Add(mapServicePlayerUpdate);
                disposables.Add(mapDataRequests);
            });
        }

        public void DetachGateway()
        {
            disposables.ForEach(v => v.Dispose());
        }

        //TODO: Check for multiple logins via accumulator function on inboundtraffic
        IObservable<IUser> LoggedInUser(IObservable<IMessage> inboundtraffic)
        {
            return inboundtraffic.OfType<AccountMessage>()
                          .Where(v=> v.Context == MessageContext.LOGIN)
                          .SelectMany(v => userService.LoginUser(v.Name, v.Password))
                          .Catch<IUser, Exception>(tx =>
                          {
                              responses.OnNext(loginFail);
                              return Observable.Empty<IUser>();
                          })
                          .Do(v =>
                          {
                              if(v != null)
                                  responses.OnNext(loginSuccess);
                          });
        }

        IDisposable HandleRegister(IObservable<IMessage> inboundtraffic)
        {
            return inboundtraffic.OfType<AccountMessage>()
                                .Where(v=> v.Context == MessageContext.REGISTER)
                                .Select(v => userService.RegisterUser(v.Name, v.Password))
                                .Switch()
                                .Catch<bool, Exception>(tx =>
                                {
                                    responses.OnNext(registerFail);
                                    return Observable.Return(false);
                                })
                                .Subscribe(v =>
                                {
                                    if (v)
                                    {
                                        responses.OnNext(registerSuccess);
                                    }
                                });
        }
        
        IObservable<PlusCode> LatestClientLocation(IObservable<IMessage> inboundtraffic)
        {
            return inboundtraffic.OfType<UserGpsMessage>()
                          .Select(v => { return new GPS(v.Lat, v.Lon); })
                          .Select(v => v.GetPlusCode(10))
                          .DistinctUntilChanged();
        }

        
    }
}

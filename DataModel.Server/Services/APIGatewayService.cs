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
using DataModel.Server.Model;
using System.Linq;

namespace DataModel.Server.Services
{
    public class APIGatewayService
    {

        readonly Subject<IMessage> responses = new Subject<IMessage>();
        readonly UserAccountService userService;
        readonly MapContentService mapService;
        readonly ResourceSpawnService resourceSpawnService;
        readonly InventoryService inventoryService;
        readonly List<IDisposable> disposables = new List<IDisposable>();

        readonly static UserActionMessage loginFail = new UserActionMessage()
        {
            MessageContext = MessageContext.LOGIN,
            MessageInfo = MessageInfo.LOGINFAIL,
            MessageState = MessageState.ERROR,
            MessageType = MessageType.RESPONSE
        };
        readonly static InventoryContentMessage contentRetrievalFail = new InventoryContentMessage()
        {
            InventoryContent = null,
            InventoryOwner = null,
            Type = MessageType.RESPONSE,
            MessageState = MessageState.ERROR

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
        static UserActionMessage loginSuccessWithId(IUser user)
        {
            var success = loginSuccess;
            success.MessageInfo = MessageInfo.USERID;
            success.AdditionalInfo = user.UserId;
            return success;
        }
        static InventoryContentMessage ContentResponse(byte[] ownerId, Dictionary<ResourceType, int> resources)
        {
            return new InventoryContentMessage() { InventoryContent = resources.ToList(), InventoryOwner = ownerId, MessageState = MessageState.SUCCESS, Type = MessageType.RESPONSE };
        }
        static MapContentTransactionMessage MapContentTransactionFail(byte[] targetId)
        {
            return new MapContentTransactionMessage() { MapContentId = targetId, MessageState = MessageState.ERROR, MessageType = MessageType.RESPONSE };
        }
        public APIGatewayService(UserAccountService userService, MapContentService mapService, ResourceSpawnService spawnService, InventoryService inventoryService)
        {
            this.userService = userService;
            this.mapService = mapService;
            this.inventoryService = inventoryService;
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


                var spawnDisposable = resourceSpawnService.AddMovableRessourceSpawnArea(v.UserId, LatestClientLocation(inboundtraffic));

                var handleInventoryRequests = HandleInventoryRequests(v, inboundtraffic);
                var handlePickupRequests = HandleMapContentPickup(v, inboundtraffic);

                disposables.Add(handleInventoryRequests);
                disposables.Add(mapServicePlayerUpdate);
                disposables.Add(mapDataRequests);
                disposables.Add(spawnDisposable);
                disposables.Add(handleInventoryRequests);
                disposables.Add(handlePickupRequests);
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
                          .Where(v => v.Context == MessageContext.LOGIN)
                          .SelectMany(v => userService.LoginUser(v.Name, v.Password))
                          .Catch<IUser, Exception>(tx =>
                          {
                              responses.OnNext(loginFail);
                              return Observable.Empty<IUser>();
                          })
                          .Do(v =>
                          {
                              if (v != null)
                                  responses.OnNext(loginSuccessWithId(v));
                          });
        }

        IDisposable HandleInventoryRequests(IUser user, IObservable<IMessage> inboundtraffic)
        {
            return inboundtraffic.OfType<InventoryContentMessage>()
                          .Where(v => v.Type == MessageType.REQUEST)
                          .SelectMany(v => 
                          {
                              return inventoryService.RequestContainerInventory(user.UserId, v.InventoryOwner).Catch<(Dictionary<ResourceType, int>, byte[]), Exception>(v2 =>
                              {
                                  responses.OnNext(contentRetrievalFail);
                                  return Observable.Empty<(Dictionary<ResourceType, int>, byte[])>();
                              });
                          })
                          .Subscribe(v =>
                          {
                             responses.OnNext(ContentResponse(v.Item2, v.Item1));
                          });
        }
        IDisposable HandleMapContentPickup(IUser user, IObservable<IMessage> inboundtraffic)
        {
            return inboundtraffic.OfType<MapContentTransactionMessage>()
                                      .Where(v => v.MessageType == MessageType.REQUEST)
                                      .SelectMany(v => 
                                      { 
                                          return inventoryService.MapContentPickUp(user.UserId, v.MapContentId).Catch<(bool, byte[]), Exception>(v2 =>
                                          {
                                          responses.OnNext(MapContentTransactionFail(null));
                                          return Observable.Empty<(bool, byte[])>();
                                          }); 
                                      })
                                      .Subscribe(v =>
                                      {
                                           responses.OnNext(new MapContentTransactionMessage() { MessageType = MessageType.RESPONSE, MapContentId = v.Item2, MessageState = MessageState.SUCCESS });
                                      });
        }

        IDisposable HandleRegister(IObservable<IMessage> inboundtraffic)
        {
            return inboundtraffic.OfType<AccountMessage>()
                                .Where(v => v.Context == MessageContext.REGISTER)
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

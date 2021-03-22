using DataModel.Common;
using DataModel.Common.GameModel;
using DataModel.Common.Messages;
using DataModel.Server.Model;
using DotNetty.Buffers;
using MessagePack;
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
    //TODO: Refactor architecture!
    public class APIGatewayService
    {

        readonly Subject<IMessage> responses = new Subject<IMessage>();
        readonly UserAccountService userService;
        readonly MapContentService mapService;
        readonly ResourceSpawnService resourceSpawnService;
        readonly InventoryService inventoryService;
        readonly QuestService questService;
        readonly List<IDisposable> disposables = new List<IDisposable>();



        readonly MessagePackSerializerOptions lz4Options;

        public APIGatewayService(UserAccountService userService, MapContentService mapService, ResourceSpawnService spawnService, InventoryService inventoryService, QuestService questService, ref MessagePackSerializerOptions options)
        {
            this.userService = userService;
            this.mapService = mapService;
            this.inventoryService = inventoryService;
            this.resourceSpawnService = spawnService;
            this.questService = questService;
            lz4Options = options;

        }
        public IObservable<IMessage> GatewayResponse => responses.ObserveOn(TaskPoolScheduler.Default).AsObservable();
        public void AttachGateway(IObservable<byte[]> inboundBytes)
        {
            var inboundtraffic = inboundBytes.ObserveOn(TaskPoolScheduler.Default)
                   .Select(v => Observable.Defer(() =>
                   {
                       return Observable.Start(() =>
                       {
                           return MessagePackSerializer.Deserialize<IMessage>(v, lz4Options);
                       })
                       .Catch<IMessage, Exception>(e =>
                       {
                           return Observable.Empty<IMessage>();
                       });
                   }))
                   .SelectMany(v => v);

            disposables.Add(HandleRegister(inboundtraffic));

            LoggedInUser(inboundtraffic).Where(v => v != null)
                                        .Take(1)
                                        .Do(v => { responses.OnNext(GatewayResponses.LoginSuccessWithId(v)); })
                                        .Subscribe(v =>
                                        {
                                            var mapServicePlayerUpdate = mapService.AddMapContent(v.AsMapContent(), LatestClientLocation(inboundtraffic));
                                            var mapDataRequests = LatestClientLocation(inboundtraffic).Sample(TimeSpan.FromSeconds(3))
                                                                                                      .Select(v2 =>
                                                                                                      {
                                                                                                          return mapService.GetMapUpdate(v2.Code).Catch<BatchContentMessage, Exception>(e => Observable.Empty<BatchContentMessage>());
                                                                                                      })
                                                                                                      .Switch()
                                                                                                      .Subscribe(v2 => responses.OnNext(v2));
                                            //TODO: Equals and Hashcode for DistinctUntilChanged updates.


                                            var spawnDisposable = resourceSpawnService.AddMovableResourceSpawnArea(v.UserId, LatestClientLocation(inboundtraffic));
                                            var spawnQuestDisposable = resourceSpawnService.AddMoveableQuestResourceSpawnArea(v.UserId, LatestClientLocation(inboundtraffic));


                                            var handleInventoryRequests = HandleInventoryRequests(v, inboundtraffic);
                                            var handlePickupRequests = HandleMapContentPickup(v, inboundtraffic);
                                            var handleQuestGenerationRequests = HandleQuestGenerationRequests(v, inboundtraffic);
                                            var handleQuestList = HandleQuestlistRequest(v, inboundtraffic);


                                            disposables.Add(spawnDisposable);
                                            disposables.Add(spawnQuestDisposable);

                                            disposables.Add(userService.LogOffUseronDispose(v));
                                            disposables.Add(handleQuestList);
                                            disposables.Add(handleQuestGenerationRequests);
                                            
                                            disposables.Add(handleInventoryRequests);
                                            disposables.Add(mapServicePlayerUpdate);
                                            disposables.Add(mapDataRequests);
                                            
                                            disposables.Add(handleInventoryRequests);
                                            disposables.Add(handlePickupRequests);
                                            disposables.Add(HandleQuestTurnIn(v, inboundtraffic));
                                        });

        }

        public void DetachGateway()
        {
            disposables.ForEach(v => v.Dispose());
        }

        IObservable<IUser> LoggedInUser(IObservable<IMessage> inboundtraffic)
        {
            return inboundtraffic.OfType<AccountMessage>()
                          .Where(v => v.Context == MessageContext.LOGIN)
                          .SelectMany(v =>
                          {
                              return userService.LoginUser(v.Name, v.Password).Catch<IUser, Exception>(tx =>
                              {
                                  responses.OnNext(GatewayResponses.LoginFail);
                                  return Observable.Empty<IUser>();
                              });
                          });

        }

        IDisposable HandleInventoryRequests(IUser user, IObservable<IMessage> inboundtraffic)
        {
            return inboundtraffic.OfType<InventoryContentMessage>()
                          .Where(v => v.Type == MessageType.REQUEST)
                          .SelectMany(v =>
                          {
                              return inventoryService.RequestContainerInventory(user.UserId, v.InventoryOwner).Catch<(Dictionary<InventoryType, int>, byte[]), Exception>(v2 =>
                              {
                                  responses.OnNext(GatewayResponses.ContentRetrievalFail);
                                  return Observable.Empty<(Dictionary<InventoryType, int>, byte[])>();
                              });
                          })
                          .Subscribe(v =>
                          {
                              responses.OnNext(GatewayResponses.ContentResponse(v.Item2, v.Item1));
                          });
        }

        IDisposable HandleQuestTurnIn(IUser user, IObservable<IMessage> inboundtraffic)
        {
            return inboundtraffic.OfType<TurnInQuestMessage>()
                                     .Where(v => v.MessageType == MessageType.REQUEST)
                                     .Where(v => v.QuestId != null)
                                     .SelectMany(v =>
                                     {
                                         return questService.TurnInQuest(user, v.QuestId).Catch<bool, Exception>(v2 =>
                                         {
                                             responses.OnNext(GatewayResponses.TurnInFail);
                                             return Observable.Empty<bool>();
                                         });
                                     })
                                     .Subscribe(v =>
                                     {
                                         if (v)
                                         {
                                             responses.OnNext(GatewayResponses.TurnInSuccess);
                                         }
                                         else
                                         {
                                             responses.OnNext(GatewayResponses.TurnInFail);
                                         }
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

                                              return Observable.Empty<(bool, byte[])>();
                                          });
                                      })
                                      .Subscribe(v =>
                                      {
                                          if (v.Item1 == true)
                                          {
                                              responses.OnNext(new MapContentTransactionMessage() { MessageType = MessageType.RESPONSE, MapContentId = v.Item2, MessageState = MessageState.SUCCESS });
                                          }
                                          else
                                          {
                                              responses.OnNext(new MapContentTransactionMessage() { MessageType = MessageType.RESPONSE, MapContentId = v.Item2, MessageState = MessageState.ERROR });
                                          }
                                      });
        }
        IDisposable HandleQuestlistRequest(IUser user, IObservable<IMessage> inboundtraffic)
        {
            return inboundtraffic.OfType<ActiveUserQuestsMessage>().Where(v => v.MessageType == MessageType.REQUEST)
                                                                   .SelectMany(v =>
                                                                   {
                                                                       return questService.RequestActiveQuests(user.UserId)
                                                                                          .Catch<List<QuestContainer>, Exception>(e => Observable.Empty<List<QuestContainer>>());

                                                                   })
                                                                   .Subscribe(v =>
                                                                   {
                                                                       responses.OnNext(GatewayResponses.ActiveQuestListResponse(v));
                                                                   });
        }
        IDisposable HandleQuestGenerationRequests(IUser user, IObservable<IMessage> inboundtraffic)
        {
            var clientLocation = LatestClientLocation(inboundtraffic);
            return inboundtraffic.OfType<QuestRequestMessage>().Where(v => v.MessageType == MessageType.REQUEST)
                                                                      .WithLatestFrom(clientLocation, (req, location) => new { req, location })
                                                                      .SelectMany(v =>
                                                                      {
                                                                          return questService.GenerateNewQuest(user, v.req.QuestContainerId, v.location.Code)
                                                                                             .Catch<QuestContainer, Exception>(e => Observable.Empty<QuestContainer>());
                                                                      })
                                                                      .Subscribe(v =>
                                                                      {
                                                                          if (v != null)
                                                                          {
                                                                              Debug.Assert(v.Quest != null);
                                                                              responses.OnNext(GatewayResponses.QuestRequestResponse(v.Quest));
                                                                          }
                                                                          else
                                                                          {
                                                                              responses.OnNext(GatewayResponses.QuestRequestResponse(null));
                                                                          }
                                                                      });
        }

        IDisposable HandleRegister(IObservable<IMessage> inboundtraffic)
        {
            return inboundtraffic.OfType<AccountMessage>()
                                .Where(v => v.Context == MessageContext.REGISTER)
                                .SelectMany(v =>
                                {
                                    return userService.RegisterUser(v.Name, v.Password).Catch<bool, Exception>(tx => Observable.Return(false));
                                })
                                .Subscribe(v =>
                                {
                                    if (v)
                                    {
                                        responses.OnNext(GatewayResponses.RegisterSuccess);
                                    }
                                    else
                                    {
                                        responses.OnNext(GatewayResponses.RegisterFail);
                                    }
                                });
        }

        IObservable<PlusCode> LatestClientLocation(IObservable<IMessage> inboundtraffic)
        {
            return inboundtraffic.ObserveOn(TaskPoolScheduler.Default).OfType<UserGpsMessage>()
                          .Select(v => { return new GPS(v.Lat, v.Lon); })
                          .Select(v => v.GetPlusCode(10));
        }


    }
}

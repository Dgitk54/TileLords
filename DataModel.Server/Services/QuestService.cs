using DataModel.Common;
using DataModel.Server.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace DataModel.Server.Services
{
    public class QuestService
    {
        public IObservable<bool> TurnInQuest(IUser player, byte[] questId)
        {
            return Observable.Create<bool>(v =>
            {
                //Perform read only checks if player is capable of turning quest in: 

                var userQuests = MongoDBFunctions.GetQuestsForUser(player.UserId).Result;
                var enumerable = userQuests.Where(e => e.Quest.QuestId.SequenceEqual(questId));
                if(enumerable.Count() == 0)
                {
                    v.OnNext(false);
                    v.OnCompleted();
                    return Disposable.Empty;
                }
                if(enumerable.Count() > 1)
                {
                    throw new Exception("duplicate state");
                }
                var inventory = MongoDBFunctions.RequestInventory(player.UserId, player.UserId).Result;

                var quest = enumerable.First();

                var itemKey = quest.Quest.GetQuestItemDictionaryKey();
                var questItems = 0;
                var userHasQuestItems = inventory.TryGetValue(itemKey, out questItems);
                
                if (!userHasQuestItems)
                {
                    v.OnNext(false);
                    v.OnCompleted();
                    return Disposable.Empty;
                }
                
                if(questItems < quest.Quest.RequiredAmountForCompletion)
                {
                    v.OnNext(false);
                    v.OnCompleted();
                    return Disposable.Empty;
                }


                //Performa action with write locks:
                v.OnNext(MongoDBFunctions.TurnInQuest(player.UserId, questId).Result);
                v.OnCompleted();
                return Disposable.Empty;
            });
        }

        public IObservable<QuestContainer> GenerateNewQuest(IUser player, byte[] questTarget, string startLocation)
        {
            return Observable.Create<QuestContainer>(v =>
            {
                //TODO: Add TownQuests with questtargets
                if (questTarget == null) //Null due to only level 1 quests are supported yet.
                {
                    var randomQuest = ServerFunctions.GetLevel1QuestForUser(startLocation.From10String());
                    var wrappedLevel1Quest = ServerFunctions.WrapLevel1Quest(randomQuest, player.UserId);
                    var result = MongoDBFunctions.AddQuestForUser(player.UserId, wrappedLevel1Quest).Result;
                    if (result)
                    {
                        v.OnNext(wrappedLevel1Quest);
                        v.OnCompleted();
                        return Disposable.Empty;
                    }
                    else
                    {
                        v.OnError(new Exception("Error generating quest"));
                        return Disposable.Empty;
                    }
                }
                else
                {
                    v.OnError(new NotImplementedException());
                    return Disposable.Empty;
                }
            });
        }
        public IObservable<List<QuestContainer>> RequestActiveQuests(byte[] playerId)
        {
            return Observable.Create<List<QuestContainer>>(v =>
            {
                var quests = MongoDBFunctions.GetQuestsForUser(playerId).Result;
                v.OnNext(quests);
                v.OnCompleted();
                return Disposable.Empty;
            });
        }
    }
}

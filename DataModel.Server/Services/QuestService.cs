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
            return Observable.Create<bool>(async v =>
            {
                //Perform read only checks if player is capable of turning quest in: 

                var userQuests = await MongoDBFunctions.GetQuestsForUser(player.UserId);
                var enumerable = userQuests.Where(e => e.Quest.QuestId.SequenceEqual(questId));
                if (enumerable.Count() == 0)
                {
                    v.OnNext(false);
                    v.OnCompleted();
                    return Disposable.Empty;
                }
                if (enumerable.Count() > 1)
                {
                    throw new Exception("duplicate state");
                }
                var inventory = await MongoDBFunctions.RequestInventory(player.UserId, player.UserId);

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

                if (questItems < quest.Quest.RequiredAmountForCompletion)
                {
                    v.OnNext(false);
                    v.OnCompleted();
                    return Disposable.Empty;
                }


                //Performa action with write locks:
                v.OnNext(await MongoDBFunctions.TurnInQuest(player.UserId, questId));
                v.OnCompleted();
                return Disposable.Empty;
            });
        }

        public IObservable<QuestContainer> GenerateNewQuest(IUser player, byte[] questTarget, string startLocation)
        {
            return Observable.Create<QuestContainer>(async v =>
            {
                //TODO: Add TownQuests with questtargets
                if (questTarget == null) //Null due to only level 1 quests are supported yet.
                {
                    var randomQuest = ServerFunctions.GetLevel1QuestForUser(startLocation.From10String());
                    var wrappedLevel1Quest = ServerFunctions.WrapLevel1Quest(randomQuest, player.UserId);
                    var result = await MongoDBFunctions.AddQuestForUser(player.UserId, wrappedLevel1Quest);
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
            return Observable.Create<List<QuestContainer>>(async v =>
            {
                var quests = await MongoDBFunctions.GetQuestsForUser(playerId);
                v.OnNext(quests);
                v.OnCompleted();
                return Disposable.Empty;
            });
        }
    }
}

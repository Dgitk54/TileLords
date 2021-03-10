using DataModel.Common;
using DataModel.Server.Model;
using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace DataModel.Server.Services
{
    public class QuestService
    {
        //TODO: QuestTurnin


        public IObservable<QuestContainer> GenerateNewQuest(IUser player, byte[] questTarget, string startLocation)
        {
            return Observable.Create<QuestContainer>(v =>
            {
                if (questTarget == null)
                {
                    var randomQuest = ServerFunctions.GetLevel1QuestForUser(startLocation.From10String());
                    var wrappedLevel1Quest = ServerFunctions.WrapLevel1Quest(randomQuest, player.UserId);
                    var result = DataBaseFunctions.AddQuestForUser(player.UserId, wrappedLevel1Quest);
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
                var quests = DataBaseFunctions.GetQuestsForUser(playerId);
                v.OnNext(quests);
                v.OnCompleted();
                return Disposable.Empty;
            });
        }
    }
}

using DataModel.Common;
using DataModel.Common.GameModel;
using DataModel.Common.Messages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace DataModel.Server.Model
{
    public static class GatewayResponses
    {
        public static readonly UserActionMessage loginFail = new UserActionMessage()
        {
            MessageContext = MessageContext.LOGIN,
            MessageInfo = MessageInfo.LOGINFAIL,
            MessageState = MessageState.ERROR,
            MessageType = MessageType.RESPONSE
        };
        public static ReadOnlyCollection<byte> loginFailBytes => Array.AsReadOnly(loginFail.ToJsonPayload()); 
        
        public static readonly UserActionMessage loginSuccess = new UserActionMessage()
        {
            MessageContext = MessageContext.LOGIN,
            MessageInfo = MessageInfo.NONE,
            MessageState = MessageState.SUCCESS,
            MessageType = MessageType.RESPONSE
        };
        public static ReadOnlyCollection<byte> loginSuccessBytes => Array.AsReadOnly(loginSuccess.ToJsonPayload());

        public static readonly UserActionMessage registerFail = new UserActionMessage()
        {
            MessageContext = MessageContext.REGISTER,
            MessageInfo = MessageInfo.NONE,
            MessageState = MessageState.ERROR,
            MessageType = MessageType.RESPONSE
        };
        public static ReadOnlyCollection<byte> registerFailBytes => Array.AsReadOnly(registerFail.ToJsonPayload());
        public static readonly UserActionMessage registerSuccess = new UserActionMessage()
        {
            MessageContext = MessageContext.REGISTER,
            MessageInfo = MessageInfo.NONE,
            MessageState = MessageState.SUCCESS,
            MessageType = MessageType.RESPONSE
        };

        public static ReadOnlyCollection<byte> registerSuccessBytes => Array.AsReadOnly(registerSuccess.ToJsonPayload());
        
         

        public static readonly InventoryContentMessage contentRetrievalFail = new InventoryContentMessage()
        {
            InventoryContent = null,
            InventoryOwner = null,
            Type = MessageType.RESPONSE,
            MessageState = MessageState.ERROR

        };
        public static UserActionMessage loginSuccessWithId(IUser user)
        {
            var success = loginSuccess;
            success.MessageInfo = MessageInfo.USERID;
            success.AdditionalInfo = user.UserId;
            return success;
        }
        public static InventoryContentMessage ContentResponse(byte[] ownerId, Dictionary<InventoryType, int> resources)
        {
            return new InventoryContentMessage() { InventoryContent = resources.ToList(), InventoryOwner = ownerId, MessageState = MessageState.SUCCESS, Type = MessageType.RESPONSE };
        }
        public static MapContentTransactionMessage MapContentTransactionFail(byte[] targetId)
        {
            return new MapContentTransactionMessage() { MapContentId = targetId, MessageState = MessageState.ERROR, MessageType = MessageType.RESPONSE };
        }
        public static QuestRequestMessage QuestRequestResponse(Quest quest)
        {
            if (quest != null)
            {
                return new QuestRequestMessage() { MessageState = MessageState.SUCCESS, MessageType = MessageType.RESPONSE, Quest = quest, QuestContainerId = null };

            }
            else
            {
                return new QuestRequestMessage() { MessageState = MessageState.ERROR, QuestContainerId = null, Quest = null, MessageType = MessageType.RESPONSE };
            }
        }

        public static TurnInQuestMessage TurnInFail() 
        => new TurnInQuestMessage() { QuestId = null, MessageState = MessageState.ERROR, MessageType = MessageType.RESPONSE };

        public static TurnInQuestMessage TurnInSuccess() => new TurnInQuestMessage() { QuestId = null, MessageState = MessageState.SUCCESS, MessageType = MessageType.RESPONSE };


        public static ActiveUserQuestsMessage ActiveQuestListResponse(List<QuestContainer> quests)
        {
            if (quests == null)
                return new ActiveUserQuestsMessage() { CurrentUserQuests = null, MessageState = MessageState.ERROR, MessageType = MessageType.RESPONSE };

            var activeQuests = quests.Where(v => !v.QuestHasExpired)
                  .Where(v => v.Quest != null);
            if (activeQuests.Any())
            {
                var questList = activeQuests.Select(v => v.Quest).ToList();
                var msg = new ActiveUserQuestsMessage() { CurrentUserQuests = questList, MessageState = MessageState.SUCCESS, MessageType = MessageType.RESPONSE };
                return msg;
            }
            else
            {
                return new ActiveUserQuestsMessage() { CurrentUserQuests = null, MessageState = MessageState.SUCCESS, MessageType = MessageType.RESPONSE };
            }
        }
        
       
    }
}

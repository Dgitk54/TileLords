using DataModel.Common.GameModel;
using DataModel.Common.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataModel.Server.Model
{
    public static class GatewayResponses
    {
        public readonly static UserActionMessage loginFail = new UserActionMessage()
        {
            MessageContext = MessageContext.LOGIN,
            MessageInfo = MessageInfo.LOGINFAIL,
            MessageState = MessageState.ERROR,
            MessageType = MessageType.RESPONSE
        };
        public readonly static InventoryContentMessage contentRetrievalFail = new InventoryContentMessage()
        {
            InventoryContent = null,
            InventoryOwner = null,
            Type = MessageType.RESPONSE,
            MessageState = MessageState.ERROR

        };

        public readonly static UserActionMessage loginSuccess = new UserActionMessage()
        {
            MessageContext = MessageContext.LOGIN,
            MessageInfo = MessageInfo.NONE,
            MessageState = MessageState.SUCCESS,
            MessageType = MessageType.RESPONSE
        };

        public readonly static UserActionMessage registerFail = new UserActionMessage()
        {
            MessageContext = MessageContext.REGISTER,
            MessageInfo = MessageInfo.NONE,
            MessageState = MessageState.ERROR,
            MessageType = MessageType.RESPONSE
        };
        public readonly static UserActionMessage registerSuccess = new UserActionMessage()
        {
            MessageContext = MessageContext.REGISTER,
            MessageInfo = MessageInfo.NONE,
            MessageState = MessageState.SUCCESS,
            MessageType = MessageType.RESPONSE
        };
        public static UserActionMessage loginSuccessWithId(IUser user)
        {
            var success = loginSuccess;
            success.MessageInfo = MessageInfo.USERID;
            success.AdditionalInfo = user.UserId;
            return success;
        }
        public static InventoryContentMessage ContentResponse(byte[] ownerId, Dictionary<InventoryType,int> resources)
        {
            return new InventoryContentMessage() { InventoryContent = resources.ToList(), InventoryOwner = ownerId, MessageState = MessageState.SUCCESS, Type = MessageType.RESPONSE };
        }
        public static MapContentTransactionMessage MapContentTransactionFail(byte[] targetId)
        {
            return new MapContentTransactionMessage() { MapContentId = targetId, MessageState = MessageState.ERROR, MessageType = MessageType.RESPONSE };
        }
        public static QuestRequestMessage QuestRequestResponse(Quest quest)
        {
            if(quest != null)
            {
                return new QuestRequestMessage() { MessageState = MessageState.SUCCESS, MessageType = MessageType.RESPONSE, Quest = quest, QuestContainerId = null };

            }
            else
            {
                return new QuestRequestMessage() { MessageState = MessageState.ERROR, QuestContainerId = null, Quest = null, MessageType = MessageType.RESPONSE };
            }
        }
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

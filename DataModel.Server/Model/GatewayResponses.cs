﻿using DataModel.Common.GameModel;
using DataModel.Common.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataModel.Server.Model
{
    /// <summary>
    /// Static class for preconfigured server responses for Clientrequests.
    /// </summary>
    public static class GatewayResponses
    {
        public static UserActionMessage LoginFail = new UserActionMessage()
        {
            MessageContext = MessageContext.LOGIN,
            MessageInfo = MessageInfo.LOGINFAIL,
            MessageState = MessageState.ERROR,
            MessageType = MessageType.RESPONSE
        };
        public static UserActionMessage LoginSuccess = new UserActionMessage()
        {
            MessageContext = MessageContext.LOGIN,
            MessageInfo = MessageInfo.NONE,
            MessageState = MessageState.SUCCESS,
            MessageType = MessageType.RESPONSE
        };
        public static UserActionMessage RegisterFail = new UserActionMessage()
        {
            MessageContext = MessageContext.REGISTER,
            MessageInfo = MessageInfo.NONE,
            MessageState = MessageState.ERROR,
            MessageType = MessageType.RESPONSE
        };
        public static UserActionMessage RegisterSuccess = new UserActionMessage()
        {
            MessageContext = MessageContext.REGISTER,
            MessageInfo = MessageInfo.NONE,
            MessageState = MessageState.SUCCESS,
            MessageType = MessageType.RESPONSE
        };
        public static InventoryContentMessage ContentRetrievalFail = new InventoryContentMessage()
        {
            InventoryContent = null,
            InventoryOwner = null,
            Type = MessageType.RESPONSE,
            MessageState = MessageState.ERROR

        };
        public static UserActionMessage LoginSuccessWithId(IUser user)
        {
            var success = LoginSuccess;
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

        public static TurnInQuestMessage TurnInFail
        = new TurnInQuestMessage() { QuestId = null, MessageState = MessageState.ERROR, MessageType = MessageType.RESPONSE };

        public static TurnInQuestMessage TurnInSuccess = new TurnInQuestMessage() { QuestId = null, MessageState = MessageState.SUCCESS, MessageType = MessageType.RESPONSE };


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

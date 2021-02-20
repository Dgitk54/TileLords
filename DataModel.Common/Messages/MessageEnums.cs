using System;
using System.Collections.Generic;
using System.Text;

namespace DataModel.Common.Messages
{
    public enum MessageType
    {
        REQUEST,
        RESPONSE
    }
    public enum MessageContext
    {
        REGISTER,
        LOGIN
    }
    public enum MessageState
    {
        ERROR,
        SUCCESS
    }
    public enum MessageInfo
    {
        NONE,
        NAMETAKEN,
        LOGINFAIL
    }
    public enum ContentType
    {
        RESSOURCE,
        PLAYER,
        QUEST
    }

    public enum ResourceType
    {
        NONE,
        APPLE,
        BANANA,
        WOOD,
        SHEEP,
        CAMEL
    }
}

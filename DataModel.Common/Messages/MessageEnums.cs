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
        LOGIN,
        TRANSACTION
    }
    public enum MessageState
    {
        ERROR,
        SUCCESS,
        NONE
    }
    public enum MessageInfo
    {
        NONE,
        NAMETAKEN,
        LOGINFAIL,
        USERID
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
        AMETHYST,
        BERRY,
        CARROT,
        CHICKEN,
        COAL,
        COCONUT,
        COFFEE,
        COPPER,
        DIAMOND,
        EGG,
        EMERALD,
        GOLD,
        GRAPE,
        IRON,
        LEATHER,
        MEAT,
        MILK,
        ORANGE,
        PELT,
        POTATO,
        PUMPKIN,
        RUBY,
        SALAD,
        SAND,
        SILVER,
        SPICE,
        STONE,
        TEA,
        TOMATO,
        WHEAT,
        WOOL,
    }
}

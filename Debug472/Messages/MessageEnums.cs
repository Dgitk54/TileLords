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
    
}

namespace DataModel.Common.Messages
{
    [MessagePack.Union(0, typeof(AccountMessage))]
    [MessagePack.Union(1, typeof(ActiveUserQuestsMessage))]
    [MessagePack.Union(2, typeof(BatchContentMessage))]
    [MessagePack.Union(3, typeof(ContentMessage))]
    [MessagePack.Union(4, typeof(InventoryContentMessage))]
    [MessagePack.Union(5, typeof(MapContentTransactionMessage))]
    [MessagePack.Union(6, typeof(QuestRequestMessage))]
    [MessagePack.Union(7, typeof(TransactionMessage))]
    [MessagePack.Union(8, typeof(TurnInQuestMessage))]
    [MessagePack.Union(9, typeof(UserActionMessage))]
    [MessagePack.Union(10, typeof(UserGpsMessage))]
    public interface IMessage
    {

    }
}

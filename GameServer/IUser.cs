namespace GameServer 
{
    public interface IUser
    {
        byte[] UserId { get; }
        string UserName { get; set; }
    }
}

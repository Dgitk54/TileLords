namespace DataModel.Server.Model
{
    public interface IUser
    {
        byte[] UserId { get; }
        string UserName { get; set; }
    }
}

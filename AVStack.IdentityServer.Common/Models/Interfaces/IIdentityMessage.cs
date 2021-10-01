namespace AVStack.IdentityServer.Models.Interfaces
{
    public interface IIdentityMessage
    {
        string Subject { get; set; }
        string FullName { get; set; }
        string EmailAddress { get; set; }
        string Callback { get; set; }
    }
}
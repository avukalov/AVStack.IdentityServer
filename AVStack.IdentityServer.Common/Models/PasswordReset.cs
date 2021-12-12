namespace AVStack.IdentityServer.Common.Models
{
    public class PasswordReset
    {
        public string FullName { get; set; }
        public string EmailAddress { get; set; }
        public string Callback { get; set; }
    }
}
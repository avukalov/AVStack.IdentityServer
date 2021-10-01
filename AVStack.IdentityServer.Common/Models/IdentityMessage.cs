using AVStack.IdentityServer.Models.Interfaces;

namespace AVStack.IdentityServer.Common.Models
{
    public class IdentityMessage : IIdentityMessage
    {
        public string Subject { get; set; }
        public string FullName { get; set; }
        public string EmailAddress { get; set; }
        public string Callback { get; set; }
    }
}
using AVStack.IdentityServer.WebApi.Models.Business.Interfaces;

namespace AVStack.IdentityServer.WebApi.Models.Events
{
    public class UserRegistrationEventArgs : System.EventArgs
    {
        public string MessageType = "user-registration"; 
        public IUser User { get; set; }
        public string ConfirmationLink { get; set; }
    }
}
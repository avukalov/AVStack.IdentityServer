using MediatR;

namespace AVStack.IdentityServer.WebApi.Models.Commands
{
    public class UserRegistrationRequest : IRequest
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Callback { get; set; }
    }
}
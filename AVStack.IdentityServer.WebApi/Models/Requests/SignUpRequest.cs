using AVStack.IdentityServer.WebApi.Models.Responses;
using MediatR;

namespace AVStack.IdentityServer.WebApi.Models.Requests
{
    public class SignUpRequest : IRequest<IdentityResponse>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }

        public string ClientUri { get; set; }
    }
}
using AVStack.IdentityServer.WebApi.Models.Responses;
using MediatR;

namespace AVStack.IdentityServer.WebApi.Models.Requests
{
    public class SignInRequest : IRequest<IdentityResponse>
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string ReturnUrl { get; set; }
        public bool RememberMe { get; set; } = false;

    }
}
using AVStack.IdentityServer.WebApi.Models.Responses;
using MediatR;

namespace AVStack.IdentityServer.WebApi.Models.Requests
{
    public class ForgotPasswordRequest : IRequest<IdentityResponse>
    {
        public string Email { get; set; }
    }
}
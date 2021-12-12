using AVStack.IdentityServer.Common.Models;
using MediatR;

namespace AVStack.IdentityServer.WebApi.Models.Requests
{
    public class PushPasswordResetRequest : PasswordReset, IRequest
    {
    }
}
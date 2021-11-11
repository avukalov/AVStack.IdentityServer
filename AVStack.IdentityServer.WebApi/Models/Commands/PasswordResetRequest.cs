using AVStack.IdentityServer.Common.Models;
using MediatR;

namespace AVStack.IdentityServer.WebApi.Models.Commands
{
    public class PasswordResetRequest : PasswordReset, IRequest
    {
    }
}
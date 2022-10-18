using AVStack.IdentityServer.Common.Models;
using AVStack.IdentityServer.WebApi.Data.Entities;
using MediatR;

namespace AVStack.IdentityServer.WebApi.Models.Requests
{
    public class PushUserSignUpRequest : UserRegistration, IRequest
    {
        public UserEntity User { get; set; }
    }
}
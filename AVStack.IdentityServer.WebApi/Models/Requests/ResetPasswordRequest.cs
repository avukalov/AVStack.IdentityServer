using System;
using AVStack.IdentityServer.WebApi.Models.Responses;
using MediatR;

namespace AVStack.IdentityServer.WebApi.Models.Requests
{
    public class ResetPasswordRequest : IRequest<IdentityResponse>
    {
        public Guid UserId { get; set; }
        public string Token { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
    }
}
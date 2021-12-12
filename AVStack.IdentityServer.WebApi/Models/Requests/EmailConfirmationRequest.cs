using AVStack.IdentityServer.WebApi.Models.Responses;
using MediatR;
using System;

namespace AVStack.IdentityServer.WebApi.Models.Requests
{
    public class EmailConfirmationRequest : IRequest<IdentityResponse>
    {
        public Guid UserId { get; set; }
        public string Token { get; set; }
    }
}
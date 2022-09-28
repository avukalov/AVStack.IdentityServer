using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using AVStack.IdentityServer.Common.Enums;
using AVStack.IdentityServer.WebApi.Data.Entities;
using AVStack.IdentityServer.WebApi.Models.Requests;
using AVStack.IdentityServer.WebApi.Models.Responses;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;

namespace AVStack.IdentityServer.WebApi.Handlers
{
    public class ForgotPasswordRequestHandler : IRequestHandler<ForgotPasswordRequest, IdentityResponse>
    {
        private readonly UserManager<UserEntity> _userManager;
        private readonly IMediator _mediator;

        public ForgotPasswordRequestHandler(UserManager<UserEntity> userManager, IMediator mediator)
        {
            _userManager = userManager;
            _mediator = mediator;
        }

        public async Task<IdentityResponse> Handle(ForgotPasswordRequest request, CancellationToken cancellationToken)
        {
            var response = new IdentityResponse
                {
                    Title = nameof(ForgotPasswordRequest),
                    Succeeded = false,
                    Status = HttpStatusCode.OK,
                    Message = "Password reset link was sent to your email.",
                };

            var entity = await _userManager.FindByEmailAsync(request.Email);

            if (entity == null)
            {
                response.Status = HttpStatusCode.NotFound;
                response.Errors.Add("User", new []{ "User not found."});

                return response;
            }

            await _mediator.Send(new PushPasswordResetRequest
            {
                FullName = entity.FirstName,
                EmailAddress = entity.Email,
                Callback = await CreateCallback(EventType.PasswordRecovery, entity)
            }, cancellationToken);

            return response;
        }

        private async Task<string> CreateCallback(EventType eventType, UserEntity entity)
        {
            // TODO: Replace hardcoded uri
            return QueryHelpers
                .AddQueryString("https://localhost:5005/Account/PasswordReset", new Dictionary<string, string>
                {
                    {"token", await _userManager.GeneratePasswordResetTokenAsync(entity)},
                    {"userId", entity.Id.ToString()}
                });
        }
    }
}
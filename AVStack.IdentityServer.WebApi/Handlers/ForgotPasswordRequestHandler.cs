using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using AVStack.IdentityServer.Common.Enums;
using AVStack.IdentityServer.WebApi.Data.Entities;
using AVStack.IdentityServer.WebApi.Models.Requests;
using AVStack.IdentityServer.WebApi.Models.Responses;
using AVStack.IdentityServer.WebApi.Services.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;

namespace AVStack.IdentityServer.WebApi.Handlers
{
    public class ForgotPasswordRequestHandler : IRequestHandler<ForgotPasswordRequest, IdentityResponse>
    {
        private readonly IUserInteractionTokenService _userInteractionTokenService;
        private readonly UserManager<UserEntity> _userManager;
        private readonly IMediator _mediator;

        public ForgotPasswordRequestHandler(UserManager<UserEntity> userManager, IMediator mediator, IUserInteractionTokenService userInteractionTokenService)
        {
            _userManager = userManager;
            _mediator = mediator;
            _userInteractionTokenService = userInteractionTokenService;
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
                Callback = await _userInteractionTokenService.CreateCallbackByEventType(EventType.PasswordRecovery, entity)
            }, cancellationToken);

            return response;
        }
    }
}
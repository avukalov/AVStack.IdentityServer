using System.Net;
using System.Threading;
using System.Threading.Tasks;
using AVStack.IdentityServer.WebApi.Data.Entities;
using AVStack.IdentityServer.WebApi.Models.Requests;
using AVStack.IdentityServer.WebApi.Models.Responses;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace AVStack.IdentityServer.WebApi.Handlers
{
    public class EmailConfirmationRequestHandler : IRequestHandler<EmailConfirmationRequest, IdentityResponse>
    {
        private readonly UserManager<UserEntity> _userManager;

        public EmailConfirmationRequestHandler(UserManager<UserEntity> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IdentityResponse> Handle(EmailConfirmationRequest request, CancellationToken cancellationToken)
        {
            var response = new IdentityResponse()
            {
                Title = nameof(EmailConfirmationRequest),
                Succeeded = true,
                Status = HttpStatusCode.OK,
                Message = "Email confirmed."
            };

            var entity = await _userManager.FindByIdAsync(request.UserId.ToString());

            if (entity == null)
            {
                response.Succeeded = false;
                response.Status = HttpStatusCode.BadRequest;
                response.Errors.Add(nameof(HttpStatusCode.BadRequest), new []{ "User not found."});
                return response;
            }

            if (await _userManager.IsEmailConfirmedAsync(entity))
            {
                response.Succeeded = false;
                response.Status = HttpStatusCode.BadRequest;
                response.Errors.Add("Email", new []{ "Email is already confirmed."});

                return response;
            }

            var result = await _userManager.ConfirmEmailAsync(entity, request.Token);

            if (!result.Succeeded)
            {
                response.Succeeded = false;
                response.Status = HttpStatusCode.BadRequest;
                foreach (var error in result.Errors)
                {
                    response.Errors.Add(error.Code, new []{error.Description});
                }
            }

            return response;
        }
    }
}
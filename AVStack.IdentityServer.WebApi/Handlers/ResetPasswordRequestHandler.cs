using System.Net;
using System.Threading;
using System.Threading.Tasks;
using AVStack.IdentityServer.WebApi.Data.Entities;
using AVStack.IdentityServer.WebApi.Models.Requests;
using AVStack.IdentityServer.WebApi.Models.Responses;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace AVStack.IdentityServer.WebApi.Handlers
{
    public class ResetPasswordRequestHandler : IRequestHandler<ResetPasswordRequest, IdentityResponse>
    {
        private readonly UserManager<UserEntity> _userManager;

        public ResetPasswordRequestHandler(UserManager<UserEntity> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IdentityResponse> Handle(ResetPasswordRequest request, CancellationToken cancellationToken)
        {
            var response = new IdentityResponse
            {
                Title = nameof(ForgotPasswordRequest),
                Succeeded = false,
                Status = HttpStatusCode.OK,
                Message = "Password reset link was sent to your email.",
            };

            var entity = await _userManager.FindByIdAsync(request.UserId.ToString());

            if (entity == null)
            {
                response.Succeeded = false;
                response.Status = HttpStatusCode.BadRequest;
                response.Errors.Add("User", new []{"User not found."});
            }

            var result = await _userManager.ResetPasswordAsync(entity, request.Token, request.Password);

            if (result.Succeeded) return response;

            response.Succeeded = result.Succeeded;
            response.Status = HttpStatusCode.BadRequest;
            response.Errors.Add("ResetPassword", new []{"Something goes wrong while resetting password."});

            return response;
        }
    }
}
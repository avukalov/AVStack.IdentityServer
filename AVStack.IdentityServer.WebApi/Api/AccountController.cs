using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AVStack.IdentityServer.WebApi.Data.Entities;
using AVStack.IdentityServer.WebApi.Models.Requests;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AVStack.IdentityServer.WebApi.Api
{
    [ApiController]
    [Route("admin/account")]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<UserEntity> _userManager;
        private readonly IMediator _mediator;

        public AccountController(UserManager<UserEntity> userManager, IMediator mediator)
        {
            _userManager = userManager;
            _mediator = mediator;
        }

        [HttpPost("send-email-confirmation/{userId}")]
        public async Task<IActionResult> SendEmailConfirmation(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
            {
                return NotFound();
            }
            
            await _mediator.Send(new PushUserSignUpRequest {User = user});

            return Ok();
        }
        
        [HttpPost("change-user-password")]
        public async Task<IActionResult> ChangeUserPassword(ChangeUserPasswordRequest request)
        {
            var user = await _userManager.FindByIdAsync(request.UserId.ToString());
            if (user is null)
            {
                return NotFound();
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, request.Password);

            if (result.Succeeded) return Ok();
            
            Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return new JsonResult(result.Errors);
        }
    }
}
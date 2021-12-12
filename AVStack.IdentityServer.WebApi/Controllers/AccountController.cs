using System;
using System.Threading.Tasks;
using AVStack.IdentityServer.WebApi.Data.Entities;
using AVStack.IdentityServer.WebApi.Models.Requests;
using IdentityServer4.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;


namespace AVStack.IdentityServer.WebApi.Controllers
{
    [AllowAnonymous]
    [ApiController]
    [Route("account")]
    public class AccountController : ControllerBase
    {
        #region Fields

        private readonly IIdentityServerInteractionService _interactionService;
        private readonly SignInManager<UserEntity> _signInManager;
        private readonly IMediator _mediator;

        #endregion

        #region Constructors

        public AccountController(
            IIdentityServerInteractionService interactionService,
            SignInManager<UserEntity> signInManager,
            IMediator mediator
        )
        {
            _interactionService = interactionService;
            _signInManager = signInManager;
            _mediator = mediator;
        }

        #endregion

        #region Methods

        [HttpPost("email-confirmation")]
        public async Task<IActionResult> EmailConfirmationAsync([FromBody] EmailConfirmationRequest request)
        {
            var result = await _mediator.Send(request);
            Response.StatusCode = (int)result.Status;
            return new JsonResult(result);
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPasswordAsync([FromBody] ForgotPasswordRequest request)
        {
            var result = await _mediator.Send(request);
            Response.StatusCode = (int)result.Status;
            return new JsonResult(result);
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPasswordAsync([FromBody] ResetPasswordRequest request)
        {
            var result = await _mediator.Send(request);
            Response.StatusCode = (int)result.Status;
            return new JsonResult(result);
        }

        [HttpPost("sign-in")]
        public async Task<IActionResult> SignInAsync([FromBody] SignInRequest request)
        {
            // Since we are receiving request from spa, we do not need to handle cancel yet

            var context = await _interactionService.GetAuthorizationContextAsync(request.ReturnUrl);

            if (context == null) return Unauthorized();

            var result = await _mediator.Send(request);

            Response.StatusCode = (int) result.Status;
            return !result.Succeeded ? new JsonResult(result) : new JsonResult(new { redirectUrl = request.ReturnUrl });
        }

        [HttpPost("sign-out")]
        public async Task<IActionResult> SignOutAsync([FromBody] SignOutRequest model)
        {
            var context = await _interactionService.GetLogoutContextAsync(model.LogoutId);

            var showSignoutPrompt = context?.ShowSignoutPrompt != false;

            if (User?.Identity?.IsAuthenticated == true)
            {
                await _signInManager.SignOutAsync();
            }

            // TODO: Add support for external signout

            // Handle this response from service
            return Ok(new
            {
                ClientName = string.IsNullOrEmpty(context?.ClientName) ? context?.ClientId : context?.ClientName,
                context?.PostLogoutRedirectUri,
                context?.SignOutIFrameUrl,
                showSignoutPrompt,
                model.LogoutId
            });

        }

        [HttpPost("sign-up")]
        public async Task<IActionResult> SignUpAsync([FromBody] SignUpRequest request)
        {
            var result = await _mediator.Send(request);

            Response.StatusCode = (int)result.Status;
            return new JsonResult(result);
        }

        #endregion
    }
}
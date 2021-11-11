using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AVStack.IdentityServer.WebApi.Controllers.Validators;
using AVStack.IdentityServer.WebApi.Models.Application;
using AVStack.IdentityServer.WebApi.Services.Interfaces;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace AVStack.IdentityServer.WebApi.Controllers
{
    [AllowAnonymous]
    [ValidateModelState]
    [ApiController]
    [Route("account")]
    public class AccountController : ControllerBase
    {
        #region Fields

        private readonly IIdentityServerInteractionService _interactionService;
        private readonly IAccountService _accountService;
        private readonly IMapper _mapper;

        #endregion

        #region Constructors

        public AccountController(
            IIdentityServerInteractionService interactionService,
            IAccountService accountService,
            IMapper mapper)
        {
            _interactionService = interactionService;
            _accountService = accountService;
            _mapper = mapper;
        }

        #endregion

        #region Methods

        [HttpPost("email-confirmation")]
        public async Task<IActionResult> EmailConfirmationAsync([FromBody] EmailConfirmationModel model)
        {
            var result = await _accountService.ConfirmEmailAsync(model.UserId, model.Token);

            if (!result.Succeeded)
            {
                return BadRequest(new { errors = result.Errors.Select(e => e.Description).ToList()});
            }

            return Ok();
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPasswordAsync([FromBody] ForgotPasswordModel model)
        {
            var result = await _accountService.ForgotPasswordAsync(model.Email);

            if (!result.Succeeded)
            {
                return BadRequest(new { errors = result.Errors.Select(e => e.Description).ToList() });
            }

            return Ok();
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPasswordAsync([FromBody] ResetPasswordModel model)
        {
            var result = await _accountService.ResetPasswordAsync(model.UserId, model.Password, model.Token);

            if (!result.Succeeded)
            {
                return BadRequest(new { errors = result.Errors.Select(e => e.Description).ToList() });
            }

            return Ok();
        }

        [HttpPost("sign-in")]
        public async Task<IActionResult> SignInAsync([FromBody] SignInModel model)
        {
            // Since we are receiving request from spa, we do not need to handle cancel yet

            var context = await _interactionService.GetAuthorizationContextAsync(model.ReturnUrl);

            if (context == null) return Unauthorized();

            var result = await _accountService.LoginUserAsync(model.UserName, model.Password, model.RememberMe);

            if (!result.Succeeded)
            {
                return BadRequest(new { errors = result.Errors.Select(e => e.Description).ToList() });
            }

            return Ok(new { redirectUrl = model.ReturnUrl });
        }

        [HttpPost("sign-out")]
        public async Task<IActionResult> SignOutAsync([FromBody] SignOutModel model)
        {
            var context = await _interactionService.GetLogoutContextAsync(model.LogoutId);

            var showSignoutPrompt = context?.ShowSignoutPrompt != false;

            if (User?.Identity?.IsAuthenticated == true)
            {
                await _accountService.LogoutUserAsync();
            }

            // TODO: Add support for external signout

            return Ok();
            // new Handle this response from service
            // {
            //     ClientName = string.IsNullOrEmpty(context?.ClientName) ? context?.ClientId : context?.ClientName,
            //     context?.PostLogoutRedirectUri,
            //     context?.SignOutIFrameUrl,
            //     showSignoutPrompt,
            //     model.LogoutId
            // }
        }

        [HttpPost("sign-up")]
        public async Task<IActionResult> SignUpAsync([FromBody] SignUpModel model)
        {
            var result = await _accountService.RegisterUserAsync(_mapper.Map<User>(model));

            if (!result.Succeeded)
            {
                return BadRequest(new { errors = result.Errors.Select(e => e.Description).ToList() });
            }

            return Ok();
        }

        #endregion
    }

    #region MappingConfigurationSection

    public class AccountMappingConfigurationSection : Profile
    {
        public AccountMappingConfigurationSection()
        {
            CreateMap<SignUpModel, User>()
                .ForMember(
                    destinationMember => destinationMember.UserName,
                    memberOptions =>
                        memberOptions.MapFrom(model => EmptyUserNameResolver(model)));
        }

        private string EmptyUserNameResolver(SignUpModel user)
            => !string.IsNullOrEmpty(user.UserName) ? user.UserName : user.Email.Split('@')[0];
    }

    #endregion

    #region Classes

    public class EmailConfirmationModel
    {
        public Guid UserId { get; set; }
        public string Token { get; set; }
    }

    public class ForgotPasswordModel
    {
        public string Email { get; set; }
    }

    public class ResetPasswordModel
    {
        public Guid UserId { get; set; }
        public string Token { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
    }

    public class SignUpModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public string ClientUri { get; set; }
    }

    public class SignInModel
    {
        public string Email { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string ReturnUrl { get; set; }
        public bool RememberMe { get; set; } = false;

    }

    public class SignOutModel
    {
        public string LogoutId { get; set; }
    }

    #endregion

}
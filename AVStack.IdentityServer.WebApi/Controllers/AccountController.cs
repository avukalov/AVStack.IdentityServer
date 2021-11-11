using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using AVStack.IdentityServer.Common.Enums;
using AVStack.IdentityServer.WebApi.Controllers.Validators;
using AVStack.IdentityServer.WebApi.Data.Entities;
using AVStack.IdentityServer.WebApi.Models;
using AVStack.IdentityServer.WebApi.Models.Application;
using AVStack.IdentityServer.WebApi.Models.System;
using AVStack.IdentityServer.WebApi.Services.Interfaces;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;


namespace AVStack.IdentityServer.WebApi.Controllers
{
    [ValidateModelState]
    [ApiController]
    [Route("account")]
    public class AccountController : ControllerBase
    {
        private readonly IIdentityServerInteractionService _interactionService;
        private readonly SignInManager<UserEntity> _signInManager;
        private readonly UserManager<UserEntity> _userManager;
        private readonly IAccountService _accountService;
        private readonly IMapper _mapper;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            IIdentityServerInteractionService interactionService,
            SignInManager<UserEntity> signInManager,
            UserManager<UserEntity> userManager,
            IAccountService accountService,
            IMapper mapper,
            ILogger<AccountController> logger)
        {
            _interactionService = interactionService;
            _signInManager = signInManager;
            _userManager = userManager;
            _accountService = accountService;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpPost("email-confirmation", Name = "EmailConfirmation")]
        public async Task<IActionResult> EmailConfirmation([FromBody] EmailConfirmationModel model)
        {
            // var badRequestResponse = new BadRequestResponse();
            //
            // var user = await _userManager.FindByEmailAsync(model.Email);
            // if (await _userManager.IsEmailConfirmedAsync(user))
            // {
            //     badRequestResponse.Errors.Add("Email is already confirmed.");
            //     return BadRequest(badRequestResponse);
            // }
            //
            // var result = await _userManager.ConfirmEmailAsync(user, model.Token);
            // if (!result.Succeeded)
            // {
            //     badRequestResponse.Errors.Add("Something went wrong while processing confirmation.");
            //     return BadRequest(badRequestResponse);
            // }

            return Ok();
        }

        [HttpPost]
        [Route("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordModel model)
        {
            // var badRequestResponse = new BadRequestResponse();
            //
            // var userEntity = await _userManager.FindByEmailAsync(model.Email);
            // if (userEntity == null)
            // {
            //     badRequestResponse.Errors.Add("Email doesn't exist.");
            //     return BadRequest(badRequestResponse);
            // }
            //
            // var callback = QueryHelpers.AddQueryString(model.ClientUri, new Dictionary<string, string>()
            // {
            //     { "email", model.Email },
            //     { "token", await _userManager.GeneratePasswordResetTokenAsync(userEntity) },
            // });
            //
            // var user = _mapper.Map<User>(userEntity);
            //
            // await _accountService.PublishPasswordResetAsync(user, callback);

            return Ok();
        }

        [HttpPost]
        [Route("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null) return BadRequest("Email doesn't exist.");

            var resetPassResult = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);
            if(!resetPassResult.Succeeded)
            {
                foreach (var error in resetPassResult.Errors)
                {
                    ModelState.TryAddModelError(error.Code, error.Description);
                }
                return BadRequest(ModelState);
            }
            // return RedirectToAction(nameof(ResetPasswordConfirmation));
            return Ok();
        }

        [HttpPost]
        [Route("sign-in")]
        public async Task<IActionResult> SignInAsync([FromBody] SignInModel model)
        {
            // Since we are receiving request from spa, we do not need to handle cancel

            var context = await _interactionService.GetAuthorizationContextAsync(model.ReturnUrl);

            if (context == null) return Unauthorized();

            var result = await _accountService.LoginUserAsync(model.UserName, model.Password, model.RememberMe);
            if (!result.Succeeded)
            {
                return BadRequest(new Response
                {
                    Errors = result.Errors.Select(e => e.Description).ToList()
                });
            }

            return Ok(new { RedirectUrl = model.ReturnUrl });
        }

        [HttpPost]
        [Route("sign-out")]
        public async Task<IActionResult> SignOutAsync([FromBody] SignOutModel model)
        {
            var context = await _interactionService.GetLogoutContextAsync(model.LogoutId);

            var showSignoutPrompt = context?.ShowSignoutPrompt != false;

            if (User?.Identity?.IsAuthenticated == true)
            {
                await _signInManager.SignOutAsync();
            }

            // TODO: Add support for external signout

            return Ok(new
            {
                ClientName = string.IsNullOrEmpty(context?.ClientName) ? context?.ClientId : context?.ClientName,
                context?.PostLogoutRedirectUri,
                context?.SignOutIFrameUrl,
                showSignoutPrompt,
                model.LogoutId
            });
        }

        [HttpPost]
        [Route("sign-up")]
        public async Task<IActionResult> SignUpAsync([FromBody] SignUpModel model)
        {

            var result = await _accountService.RegisterUserAsync(_mapper.Map<User>(model));

            if (!result.Succeeded)
            {
                return BadRequest(new Response
                {
                    Errors = result.Errors.Select(e => e.Description).ToList()
                });
            }

            return Ok();
        }

        private async Task<string> GenerateCallback(EventType messageType, UserEntity userEntity)
        {
            
            return messageType switch
            {
                EventType.UserRegistration => Url.Action(nameof(EmailConfirmation), "Account",
                    new
                    {
                        userId = userEntity.Id,
                        token = _userManager.GenerateEmailConfirmationTokenAsync(userEntity).Result,
                    }, protocol: Request.Scheme),

                EventType.PasswordRecovery => Url.Action(nameof(ResetPassword), "Account",
                    new
                    {
                        email = userEntity.Email,
                        token = _userManager.GeneratePasswordResetTokenAsync(userEntity).Result,
                    }, Request.Scheme),
                
                _ => throw new ArgumentOutOfRangeException(nameof(messageType), messageType, null)
            };
        }
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
        public string Email { get; set; }
        public string Token { get; set; }
    }

    public class ForgotPasswordModel
    {
        public string ClientUri { get; set; }
        public string Email { get; set; }
    }

    public class ResetPasswordModel
    {
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public string Email { get; set; }
        public string Token { get; set; }
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
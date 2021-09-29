using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Threading.Tasks;
using AutoMapper;
using AVStack.IdentityServer.WebApi.Data.Entities;
using AVStack.IdentityServer.WebApi.Models.Business.Interfaces;
using AVStack.IdentityServer.WebApi.Services.Interfaces;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;


namespace AVStack.IdentityServer.WebApi.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AccountController : ControllerBase
    {
        private readonly SignInManager<UserEntity> _signInManager;
        private readonly UserManager<UserEntity> _userManager;
        private readonly IIdentityServerInteractionService _interactionService;
        private readonly IAccountService _accountService;
        private readonly IMapper _mapper;
        
        public AccountController(
            
            SignInManager<UserEntity> signInManager, 
            UserManager<UserEntity> userManager, 
            IIdentityServerInteractionService interactionService, 
            IAccountService accountService,
            IMapper mapper)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _interactionService = interactionService;
            _accountService = accountService;
            _mapper = mapper;
        }

        [HttpPost]
        [Route("sign-up")]
        public async Task<IActionResult> SignUpAsync(SignUpModel signUpModel)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _accountService.RegisterUserAsync(signUpModel);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.TryAddModelError(error.Code, error.Description);
                }
                return BadRequest(ModelState);
            }

            await _accountService.PublishUserRegistration(_mapper.Map<IUser>(result.UserEntity), GenerateConfirmationLink(result.UserEntity));

            return Ok();
        }
        
        [HttpPost]
        [Route("sign-in")]
        public async Task<IActionResult> SignInAsync([FromForm] SignInModel signInModel)
        {
            var context = await _interactionService.GetAuthorizationContextAsync(signInModel.ReturnUrl);
            if (!ModelState.IsValid) return BadRequest("Invalid credentials.");

            var user = await _userManager.FindByNameAsync(signInModel.UserName);
            if (!user.EmailConfirmed)
            {
                return BadRequest("Confirm your email first.");
            }
            
            var result = await _signInManager.PasswordSignInAsync(user, signInModel.Password, signInModel.RememberMe, user.LockoutEnabled);
            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Invalid credentials.");
                return BadRequest(ModelState);
            }
            
            return Ok();
        }
        
        [HttpGet("email-confirmation", Name = "EmailConfirmation")]
        public async Task<IActionResult> EmailConfirmation(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user.EmailConfirmed)
            {
                return BadRequest("You email is already confirmed.");
            }
            
            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (!result.Succeeded)
            {
                return BadRequest();
            }

            return Ok();
        }
        
        [HttpPost]
        [Route("sign-out")]
        public async Task<IActionResult> SignOutAsync()
        {
            if (User?.Identity?.IsAuthenticated == true) await _signInManager.SignOutAsync();
            return Ok();
        }
        
        private string GenerateConfirmationLink(UserEntity userEntity)
        {
            return Url.Action(
                nameof(EmailConfirmation),
                "Account",
                new
                {
                    userId = userEntity.Id,
                    token = _userManager.GenerateEmailConfirmationTokenAsync(userEntity).Result
                },
                protocol: HttpContext.Request.Scheme);
        }
    }
    
    public class SignUpModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
    }
    
    public class SignInModel
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string ReturnUrl { get; set; }
        public bool RememberMe { get; set; } = false;

    }
}
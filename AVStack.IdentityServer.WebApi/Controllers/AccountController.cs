using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Threading.Tasks;
using AVStack.IdentityServer.Constants;
using AVStack.IdentityServer.WebApi.Data.Entities;
using AVStack.MessageBus.Abstraction;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;


namespace AVStack.IdentityServer.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly SignInManager<UserEntity> _signInManager;
        private readonly UserManager<UserEntity> _userManager;
        private readonly IIdentityServerInteractionService _interactionService;
        private readonly IMessageBusFactory _busFactory;
        
        public AccountController(
            SignInManager<UserEntity> signInManager, 
            UserManager<UserEntity> userManager, 
            IIdentityServerInteractionService interactionService, 
            IMessageBusFactory busFactory)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _interactionService = interactionService;
            _busFactory = busFactory;
        }

        [HttpPost]
        [Route("SignUp")]
        public async Task<IActionResult> SignUpAsync(SignUpRequestModel newUser)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userEntity = new UserEntity
            {
                FirstName = newUser.FirstName,
                LastName = newUser.LastName,
                Email = newUser.Email,
                UserName = !string.IsNullOrEmpty(newUser.UserName) ? newUser.UserName : newUser.Email.Split('@')[0]
            };

            var result = await _userManager.CreateAsync(userEntity, newUser.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.TryAddModelError(error.Code, error.Description);
                }
                return BadRequest(ModelState);
            }
            
            await _userManager.AddToRoleAsync(userEntity, IdentityRoleDefaults.User);

            using (var producer = _busFactory.CreateProducer())
            {
                var message = JsonSerializer.Serialize(new
                {
                    FirstName = userEntity.FirstName,
                    LastName = userEntity.LastName,
                    Email = userEntity.Email,
                    UserName = userEntity.UserName,
                    EmailConfirmationLink = GenerateConfirmationLink(userEntity)
                });
                producer.Publish("confirmation.notification.newsletter", "confirmation.*.*", null, message);
            }
            
            return Ok();
        }
        
        [HttpPost]
        [Route("SignIn")]
        public async Task<IActionResult> SignInAsync([FromBody] SignInRequestModel signInModel)
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
        
        [HttpGet("EmailConfirmation", Name = "EmailConfirmation")]
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
        [Route("SignOut")]
        public async Task<IActionResult> SignOutAsync()
        {
            if (User?.Identity?.IsAuthenticated == true) await _signInManager.SignOutAsync();
            return Ok();
        }

        private string GenerateConfirmationLink(UserEntity userEntity)
        {
            var confirmationToken = _userManager.GenerateEmailConfirmationTokenAsync(userEntity).Result;
            var confirmationLink = Url.Action(
                nameof(EmailConfirmation),
                "Account",
                new
                {
                    userId = userEntity.Id,
                    token = confirmationToken
                },
                protocol: HttpContext.Request.Scheme);
            return confirmationLink;
        }
    }
    
    public class SignUpRequestModel
    {
        [Required(ErrorMessage = "First name is required.")]
        public string FirstName { get; set; }
        
        [Required(ErrorMessage = "Last name is required.")]
        public string LastName { get; set; }
        public string UserName { get; set; }
        
        [Required(ErrorMessage = "Email is required.")]
        public string Email { get; set; }
        
        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }
    
    public class SignInRequestModel
    {
        [Required(ErrorMessage = "Username is required.")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [PasswordPropertyText]
        public string Password { get; set; }

        [Required(ErrorMessage = "ReturnUrl is required.")]
        public string ReturnUrl { get; set; }
        public bool RememberMe { get; set; }
        
    }
}
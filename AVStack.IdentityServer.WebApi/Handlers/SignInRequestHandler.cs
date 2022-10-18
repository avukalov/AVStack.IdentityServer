using System.Net;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AVStack.IdentityServer.WebApi.Data.Entities;
using AVStack.IdentityServer.WebApi.Models.Requests;
using AVStack.IdentityServer.WebApi.Models.Responses;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace AVStack.IdentityServer.WebApi.Handlers
{
    public class SignInRequestHandler : IRequestHandler<SignInRequest, IdentityResponse>
    {
        private readonly SignInManager<UserEntity> _signInManager;
        private readonly UserManager<UserEntity> _userManager;
        private readonly IdentityOptions _identityOptions;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public SignInRequestHandler(SignInManager<UserEntity> signInManager, UserManager<UserEntity> userManager, IMediator mediator, IMapper mapper, IOptions<IdentityOptions> identityOptions)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _mediator = mediator;
            _mapper = mapper;
            _identityOptions = identityOptions.Value;
        }

        public async Task<IdentityResponse> Handle(SignInRequest request, CancellationToken cancellationToken)
        {
            var response = new IdentityResponse
            {
                Title = nameof(SignInRequest), 
                Succeeded = false,
            };

            var entity = await _userManager.FindByEmailAsync(request.Email);

            if (entity == null)
            {
                response.Status = HttpStatusCode.BadRequest;
                response.Message = "Invalid credentials.";
                response.Errors.Add(nameof(HttpStatusCode.BadRequest), new []{ "Invalid credentials." });
                return response;
            }

            var result = await _signInManager.PasswordSignInAsync(entity.UserName, request.Password, request.RememberMe, true);

            if (!result.Succeeded)
            {
                response.Status = HttpStatusCode.BadRequest;
                response.Message = nameof(HttpStatusCode.BadRequest);
                response.Errors.Add(nameof(HttpStatusCode.BadRequest), new []{ "Invalid credentials." });

                if (result.IsNotAllowed)
                {
                    response.Errors.Add(nameof(HttpStatusCode.Unauthorized), new []{ "User is not verified yet." });
                }

                if (result.IsLockedOut)
                {
                    response.Errors.Add(nameof(HttpStatusCode.Unauthorized), new []{ "User is locked." });
                }

                return response;
            }

            response.Succeeded = true;
            response.Status = HttpStatusCode.OK;
            response.Message = "User is successfully logged in.";
            return response;
        }
    }
}
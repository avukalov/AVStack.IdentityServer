using AutoMapper;
using AVStack.IdentityServer.Common.Enums;
using AVStack.IdentityServer.WebApi.Data.Entities;
using AVStack.IdentityServer.WebApi.Models.Requests;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using AVStack.IdentityServer.WebApi.Models.Constants;
using AVStack.IdentityServer.WebApi.Models.Responses;

namespace AVStack.IdentityServer.WebApi.Handlers
{
    public class SignUpRequestHandler : IRequestHandler<SignUpRequest, IdentityResponse>
    {
        private readonly UserManager<UserEntity> _userManager;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public SignUpRequestHandler(UserManager<UserEntity> userManager,IMediator mediator, IMapper mapper)
        {
            _userManager = userManager;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<IdentityResponse> Handle(SignUpRequest request, CancellationToken cancellationToken)
        {
            var response = new IdentityResponse() { Succeeded = false, Title = nameof(SignUpRequest) };

            response.Succeeded = true;
            response.Status = HttpStatusCode.Created;
            response.Message = "Confirmation email is sent.";

            var existingEntity = await _userManager.FindByEmailAsync(request.Email);

            if (existingEntity != null)
            {
                response.Status = HttpStatusCode.Conflict;
                response.Errors.Add(nameof(HttpStatusCode.Conflict), new [] { "User already exist" });

                return response;
            }

            var entity = new UserEntity();

            _mapper.Map(request, entity);
            
            

            var signUpResult = await _userManager.CreateAsync(entity, request.Password);

            if (!signUpResult.Succeeded)
            {
                response.Status = HttpStatusCode.BadRequest;
                foreach (var error in signUpResult.Errors)
                {
                    response.Errors.Add(error.Code, new [] { error.Description });
                }

                return response;
            }

            var identityResult = await _userManager.AddToRoleAsync(entity, IdentityRoleDefaults.User);

            if (!identityResult.Succeeded)
            {
                response.Status = HttpStatusCode.BadRequest;
                foreach (var error in signUpResult.Errors)
                {
                    response.Errors.Add(error.Code, new [] { error.Description });
                }

                return response;
            }

            var sent = await _mediator.Send(new PushUserSignUpRequest
            {
                FullName = entity.FullName,
                EmailAddress = entity.Email,
                Callback = await CreateCallback(EventType.UserRegistration, entity)
            }, cancellationToken);

            return response;
        }

        private async Task<string> CreateCallback(EventType eventType, UserEntity entity)
        {
            return QueryHelpers
                .AddQueryString(
                    "http://localhost:4200/auth/email-confirmation",
                    new Dictionary<string, string>
                    {
                        { "token", await _userManager.GenerateEmailConfirmationTokenAsync(entity) },
                        { "userId", entity.Id.ToString() }
                    });
        }
    }
}


// TODO: Move
// return eventType switch
// {
//     EventType.UserRegistration => QueryHelpers
//         .AddQueryString("http://localhost:4200/account/email-confirmation", new Dictionary<string, string>
//         {
//             {"token", await _userManager.GenerateEmailConfirmationTokenAsync(entity) },
//             {"userId", entity.Id.ToString() }
//         }),
//     EventType.PasswordRecovery => QueryHelpers
//         .AddQueryString("http://localhost:4200/account/password-reset", new Dictionary<string, string>
//         {
//             {"token", await _userManager.GeneratePasswordResetTokenAsync(entity) },
//             {"userId", entity.Id.ToString() }
//         }),
//     _ => throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null)
// };
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
using AVStack.IdentityServer.WebApi.Services.Interfaces;

namespace AVStack.IdentityServer.WebApi.Handlers
{
    public class SignUpRequestHandler : IRequestHandler<SignUpRequest, IdentityResponse>
    {
        private readonly IUserInteractionTokenService _userInteractionTokenService;
        private readonly UserManager<UserEntity> _userManager;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public SignUpRequestHandler(UserManager<UserEntity> userManager,IMediator mediator, IMapper mapper, IUserInteractionTokenService userInteractionTokenService)
        {
            _userManager = userManager;
            _mediator = mediator;
            _mapper = mapper;
            _userInteractionTokenService = userInteractionTokenService;
        }

        public async Task<IdentityResponse> Handle(SignUpRequest request, CancellationToken cancellationToken)
        {
            var response = new IdentityResponse()
            {
                Title = nameof(SignUpRequest),
                Succeeded = true,
                Status = HttpStatusCode.Created,
                Message = "User created."
            };
            
            var existingEntity = await _userManager.FindByEmailAsync(request.Email);

            if (existingEntity != null)
            {

                response.Succeeded = false;
                response.Status = HttpStatusCode.Conflict;
                response.Message = "User already exist";
                response.Errors.Add("Email", new [] { "Email is already taken" });
                return response;
            }

            var entity = new UserEntity();

            _mapper.Map(request, entity);

            var signUpResult = await _userManager.CreateAsync(entity, request.Password);

            if (!signUpResult.Succeeded)
            {
                response.Succeeded = signUpResult.Succeeded;
                response.Status = HttpStatusCode.BadRequest;
                response.Message = nameof(HttpStatusCode.BadRequest);
                foreach (var error in signUpResult.Errors)
                    response.Errors.Add(error.Code, new [] { error.Description });
                return response;
            }

            var identityResult = await _userManager.AddToRoleAsync(entity, IdentityRoleDefaults.User);

            if (!identityResult.Succeeded)
            {
                response.Succeeded = signUpResult.Succeeded;
                response.Status = HttpStatusCode.BadRequest;
                response.Message = nameof(HttpStatusCode.BadRequest);
                foreach (var error in signUpResult.Errors)
                    response.Errors.Add(error.Code, new [] { error.Description });
                return response;
            }

            // TODO: Think about saving tokens to DB and move logic to hasura webhooks
            var sent = await _mediator.Send(new PushUserSignUpRequest
            {
                FullName = entity.FullName,
                EmailAddress = entity.Email,
                Callback = await _userInteractionTokenService.CreateCallbackByEventType(EventType.UserRegistration, entity)
            }, cancellationToken);

            return response;
        }
    }
}
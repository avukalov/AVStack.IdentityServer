using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AutoMapper;
using AVStack.IdentityServer.Common.Enums;
using AVStack.IdentityServer.WebApi.Common;
using AVStack.IdentityServer.WebApi.Common.Constants;
using AVStack.IdentityServer.WebApi.Controllers;
using AVStack.IdentityServer.WebApi.Data.Entities;
using AVStack.IdentityServer.WebApi.Extensions;
using AVStack.IdentityServer.WebApi.Models.Application;
using AVStack.IdentityServer.WebApi.Models.Application.Interfaces;
using AVStack.IdentityServer.WebApi.Models.Commands;
using AVStack.IdentityServer.WebApi.Models.EventArgs;
using AVStack.IdentityServer.WebApi.Models.System;
using AVStack.IdentityServer.WebApi.Services.Interfaces;
using AVStack.MessageBus.Abstraction;
using AVStack.MessageBus.Extensions;
using IdentityServer4.Services;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;

namespace AVStack.IdentityServer.WebApi.Services
{
    public class AccountService : IAccountService
    {
        private const string Monitoring = "monitoring";
        private const string AccountUserRegistrationKey = "account.user.registration";
        private const string AccountPasswordResetKey = "account.user.password-reset";

        private readonly IIdentityServerInteractionService _interactionService;
        private readonly SignInManager<UserEntity> _signInManager;
        private readonly UserManager<UserEntity> _userManager;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public AccountService(
            IIdentityServerInteractionService interactionService,
            SignInManager<UserEntity> signInManager,
            UserManager<UserEntity> userManager,
            IMediator mediator,
            IMapper mapper)
        {
            _interactionService = interactionService;
            _signInManager = signInManager;
            _userManager = userManager;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<IdentityResultModel> RegisterUserAsync(IUser newUser, string role = null)
        {
             var userEntity = _mapper.Map<UserEntity>(newUser);

            var result = (await _userManager.CreateAsync(userEntity, newUser.Password)).ToModel();

            if (!result.Succeeded) return result;
            
            result = (await _userManager.AddToRoleAsync(userEntity, role ?? IdentityRoleDefaults.User)).ToModel();

            if (!result.Succeeded) return result;

            _mapper.Map(userEntity, newUser);
            result.User = newUser;

            await _mediator.Send(new UserRegistrationRequest
            {
                FullName = newUser.FullName,
                Email = newUser.Email,
                Callback = await GenerateCallback(EventType.UserRegistration, userEntity)
            });

            return result;
        }

        public async Task<IdentityResultModel> LoginUserAsync(string userName, string password, bool rememberMe)
        {
            return (await _signInManager.PasswordSignInAsync(userName, password, rememberMe, true)).ToModel();
        }

        // public Task PublishPasswordResetAsync(IUser userInfo, string callback)
        // {
        //     using (var producer = _busFactory.CreateProducer())
        //     {
        //         var basicProperties = producer.CreateBasicProperties();
        //         basicProperties.SetDefaultValues();
        //         basicProperties.Type = nameof(PasswordReset);
        //
        //         try
        //         {
        //             producer.Publish(
        //                 Monitoring,
        //                 routingKey:AccountPasswordResetKey,
        //                 properties:basicProperties,
        //                 JsonSerializer.Serialize(new PasswordReset()
        //             {
        //                 FullName = userInfo.FullName,
        //                 EmailAddress = userInfo.Email,
        //                 Callback = callback
        //             }));
        //         }
        //         catch (Exception e)
        //         {
        //             Console.WriteLine(e);
        //         }
        //     }
        //
        //     return Task.CompletedTask;
        // }

        private async Task<string> GenerateCallback(EventType eventType, UserEntity userEntity)
        {
            return eventType switch
            {
                EventType.UserRegistration => QueryHelpers
                    .AddQueryString("http://localhost:4200/account/email-confirmation", new Dictionary<string, string>
                    {
                        {"token", await _userManager.GenerateEmailConfirmationTokenAsync(userEntity) },
                        {"userId", userEntity.Id.ToString() }
                    }),
                EventType.PasswordRecovery => QueryHelpers
                    .AddQueryString("http://localhost:4200/account/reset-password", new Dictionary<string, string>
                    {
                        {"token", await _userManager.GeneratePasswordResetTokenAsync(userEntity) },
                        {"userId", userEntity.Id.ToString() }
                    }),
                _ => throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null)
            };
        }

    }
}
using AutoMapper;
using AVStack.IdentityServer.Common.Enums;
using AVStack.IdentityServer.WebApi.Data.Entities;
using AVStack.IdentityServer.WebApi.Extensions;
using AVStack.IdentityServer.WebApi.Models.Application.Interfaces;
using AVStack.IdentityServer.WebApi.Models.Commands;
using AVStack.IdentityServer.WebApi.Models.Constants;
using AVStack.IdentityServer.WebApi.Models.System;
using AVStack.IdentityServer.WebApi.Services.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AVStack.IdentityServer.WebApi.Services
{
    public class AccountService : IAccountService
    {
        private readonly SignInManager<UserEntity> _signInManager;
        private readonly UserManager<UserEntity> _userManager;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public AccountService(
            SignInManager<UserEntity> signInManager,
            UserManager<UserEntity> userManager,
            IMediator mediator,
            IMapper mapper)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<IdentityResultModel> ConfirmEmailAsync(Guid userId, string token)
        {
            var result = new IdentityResultModel();

            var userEntity = await _userManager.FindByIdAsync(userId.ToString());

            if (userEntity == null)
            {
                result.Errors.Add(IdentityErrorExtensions.EmailNotExist);
                return result;
            }

            if (!await _userManager.IsEmailConfirmedAsync(userEntity))
            {
                result.Errors.Add(IdentityErrorExtensions.EmailNotConfirmed);
                return result;
            }

            return (await _userManager.ConfirmEmailAsync(userEntity, token)).ToIdentityResultModel();
        }

        public async Task<IdentityResultModel> ForgotPasswordAsync(string email)
        {
            var result = new IdentityResultModel();

            var userEntity = await _userManager.FindByEmailAsync(email);

            if (userEntity == null)
            {
                result.Errors.Add(IdentityErrorExtensions.EmailNotExist);
                return result;
            }

            await _mediator.Send(new PasswordResetRequest
            {
                FullName = userEntity.FirstName,
                EmailAddress = userEntity.Email,
                Callback = await GenerateCallback(EventType.PasswordRecovery, userEntity)
            });

            return result;
        }

        public async Task<IdentityResultModel> RegisterUserAsync(IUser newUser, string role = null)
        {
            var userEntity = _mapper.Map<UserEntity>(newUser);

            var result = (await _userManager.CreateAsync(userEntity, newUser.Password)).ToIdentityResultModel();

            if (!result.Succeeded) return result;
            
            result = (await _userManager.AddToRoleAsync(userEntity, role ?? IdentityRoleDefaults.User)).ToIdentityResultModel();

            if (!result.Succeeded) return result;

            await _mediator.Send(new UserRegistrationRequest
            {
                FullName = userEntity.FullName,
                EmailAddress = userEntity.Email,
                Callback = await GenerateCallback(EventType.UserRegistration, userEntity)
            });

            return result;
        }

        public async Task<IdentityResultModel> ResetPasswordAsync(Guid userId, string password, string token)
        {
            var result = new IdentityResultModel();

            var userEntity = await _userManager.FindByIdAsync(userId.ToString());

            if (userEntity == null)
            {
                result.Errors.Add(IdentityErrorExtensions.EmailNotExist);
                return result;
            }

            return (await _userManager.ResetPasswordAsync(userEntity, token, password)).ToIdentityResultModel();
        }

        public async Task<IdentityResultModel> LoginUserAsync(string userName, string password, bool rememberMe)
        {
            return (await _signInManager.PasswordSignInAsync(userName, password, rememberMe, true)).ToIdentityResultModel();
        }
        public Task LogoutUserAsync()
        {
            return _signInManager.SignOutAsync();
        }

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
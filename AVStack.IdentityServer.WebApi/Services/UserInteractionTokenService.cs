using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AVStack.IdentityServer.Common.Enums;
using AVStack.IdentityServer.WebApi.Data.Entities;
using AVStack.IdentityServer.WebApi.Models.Options;
using AVStack.IdentityServer.WebApi.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;

namespace AVStack.IdentityServer.WebApi.Services
{
    class UserInteractionTokenService : IUserInteractionTokenService
    {
        private const string TokenString = "token";
        private const string UserIdString = "userId";

        private readonly UserManager<UserEntity> _userManager;
        private readonly AccountTokenConfirmationUrls _urls;

        public UserInteractionTokenService(
            UserManager<UserEntity> userManager,
            IOptions<AccountTokenConfirmationUrls> urls) 
        {
            _userManager = userManager;
            _urls = urls.Value;
        }

        public async Task<string> CreateCallbackByEventType(EventType eventType, UserEntity entity)
        {
            return eventType switch
            {
                EventType.UserRegistration => QueryHelpers
                    .AddQueryString(this._urls.EmailConfirmationUrl, new Dictionary<string, string>
                    {
                        {TokenString, await _userManager.GenerateEmailConfirmationTokenAsync(entity) },
                        {UserIdString, entity.Id.ToString() }
                    }),
                EventType.PasswordRecovery => QueryHelpers
                    .AddQueryString(this._urls.AccountRecoveryUrl, new Dictionary<string, string>
                    {
                        {TokenString, await _userManager.GeneratePasswordResetTokenAsync(entity) },
                        {UserIdString, entity.Id.ToString() }
                    }),
                _ => throw new ArgumentOutOfRangeException(nameof(eventType), eventType, null)
            };
        }
    }
}
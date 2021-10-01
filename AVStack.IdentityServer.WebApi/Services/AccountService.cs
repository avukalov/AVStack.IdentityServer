using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AutoMapper;
using AVStack.IdentityServer.Common.Models;
using AVStack.IdentityServer.WebApi.Common;
using AVStack.IdentityServer.WebApi.Common.Constants;
using AVStack.IdentityServer.WebApi.Controllers;
using AVStack.IdentityServer.WebApi.Data.Entities;
using AVStack.IdentityServer.WebApi.Models.Business.Interfaces;
using AVStack.IdentityServer.WebApi.Services.Interfaces;
using AVStack.MessageBus.Abstraction;
using Microsoft.AspNetCore.Identity;

namespace AVStack.IdentityServer.WebApi.Services
{
    public class AccountService : IAccountService
    {
        private const string EmailSmsViberTopicExchange = "email.sms.viber";
        private const string EmailRoutingKey = "email.*.*";
        
        private readonly UserManager<UserEntity> _userManager;
        private readonly IMessageBusFactory _busFactory;
        private readonly IMapper _mapper;

        public AccountService(
            UserManager<UserEntity> userManager, 
            IMessageBusFactory busFactory,
            IMapper mapper)
        {
            _userManager = userManager;
            _busFactory = busFactory;
            _mapper = mapper;
        }

        // TODO: Find a way how to implement events

        public async Task<IdentityResultExtended> RegisterUserAsync(SignUpModel signUpModel, string role = null)
        {
            var entity = new UserEntity();
            _mapper.Map(signUpModel, entity);

            var result = await _userManager.CreateAsync(entity, signUpModel.Password);

            if (!result.Succeeded)
            {
                return new IdentityResultExtended{ Succeeded = result.Succeeded, Errors = result.Errors.ToList() };
            }
            
            result = await _userManager.AddToRoleAsync(entity, role ?? IdentityRoleDefaults.User);
            if (!result.Succeeded)
            {
                return new IdentityResultExtended{ Succeeded = result.Succeeded, Errors = result.Errors.ToList() };
            }

            return new IdentityResultExtended{ Succeeded = result.Succeeded, Errors = result.Errors.ToList(), UserEntity = entity};
        }

        // TODO: Implement IdentityMessage
        // TODO: Leave subject to EmailService
        public Task PublishIdentityMessageAsync(string subject, string messageType, string callback, IUser userInfo)
        {
            using (var producer = _busFactory.CreateProducer())
            {
                var props = producer.CreateBasicProperties();

                props.ContentType = "application/json";
                props.Headers = new Dictionary<string, object>()
                {
                    { "message-type", messageType }
                };

                producer.Publish(EmailSmsViberTopicExchange, EmailRoutingKey, props, JsonSerializer.Serialize(
                    new
                    {
                        Subject = subject,
                        FullName = $"{userInfo.FirstName} {userInfo.LastName}",
                        EmailAddress = userInfo.Email,
                        Callback = callback
                    })
                );
            }
            return Task.FromResult(0);
        }
    }
}
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AutoMapper;
using AVStack.IdentityServer.WebApi.Common;
using AVStack.IdentityServer.WebApi.Common.Constants;
using AVStack.IdentityServer.WebApi.Controllers;
using AVStack.IdentityServer.WebApi.Data.Entities;
using AVStack.IdentityServer.WebApi.Models.Business.Interfaces;
using AVStack.IdentityServer.WebApi.Services.Interfaces;
using AVStack.MessageBus.Abstraction;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AVStack.IdentityServer.WebApi.Services
{
    public class AccountService : IAccountService
    {
        private const string EmailSmsViberExchange = "email.sms.viber";
        
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

        // TODO: Find way how to implement events 
        // public event EventHandler<UserRegistrationEventArgs> UserRegistered;
        //
        // protected virtual void OnUserRegistered(IUser user, string confirmationLink)
        // {
        //     UserRegistered?.Invoke(this, new UserRegistrationEventArgs
        //     {
        //         User = user,
        //         ConfirmationLink = confirmationLink
        //     });
        // }

        public async Task<IdentityResultExtended> RegisterUserAsync(SignUpModel signUpModel, string role = null)
        {
            var userEntity = new UserEntity
            {
                FirstName = signUpModel.FirstName,
                LastName = signUpModel.LastName,
                Email = signUpModel.Email,
                UserName = !string.IsNullOrEmpty(signUpModel.UserName) ? signUpModel.UserName : signUpModel.Email.Split('@')[0]
            };

            var result = await _userManager.CreateAsync(userEntity, signUpModel.Password);

            if (!result.Succeeded)
            {
                return new IdentityResultExtended{ Succeeded = result.Succeeded, Errors = result.Errors.ToList() };
            }
            
            result = await _userManager.AddToRoleAsync(userEntity, role ?? IdentityRoleDefaults.User);
            if (!result.Succeeded)
            {
                return new IdentityResultExtended{ Succeeded = result.Succeeded, Errors = result.Errors.ToList() };
            }

            //OnUserRegistered(_mapper.Map<IUser>(userEntity), GenerateConfirmationLink(userEntity));

            return new IdentityResultExtended{ Succeeded = result.Succeeded, Errors = result.Errors.ToList(), UserEntity = userEntity};
        }

        public Task PublishUserRegistration(IUser user, string confirmationLink)
        {
            using (var producer = _busFactory.CreateProducer())
            {

                producer.Publish(EmailSmsViberExchange, "email.*.*",null, JsonSerializer.Serialize(
                    new
                    {
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Email = user.Email,
                        Type = "email-confirmation",
                        ConfirmationLink = confirmationLink
                    })
                );
            }

            return Task.FromResult(0);
        }
    }
}
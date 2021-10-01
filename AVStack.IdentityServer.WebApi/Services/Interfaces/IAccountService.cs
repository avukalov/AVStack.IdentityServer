using System.Threading.Tasks;
using AVStack.IdentityServer.WebApi.Common;
using AVStack.IdentityServer.WebApi.Controllers;
using AVStack.IdentityServer.WebApi.Models.Business.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace AVStack.IdentityServer.WebApi.Services.Interfaces
{
    public interface IAccountService
    {
        Task<IdentityResultExtended> RegisterUserAsync(SignUpModel signUpModel, string role = null);
        Task PublishIdentityMessageAsync(string subject, string messageType, string callback, IUser user);
    }
}
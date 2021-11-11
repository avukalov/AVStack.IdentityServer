using System.Threading.Tasks;
using AVStack.IdentityServer.WebApi.Models.Application.Interfaces;
using AVStack.IdentityServer.WebApi.Models.System;

namespace AVStack.IdentityServer.WebApi.Services.Interfaces
{
    public interface IAccountService
    {
        Task<IdentityResultModel> RegisterUserAsync(IUser user, string role = null);
        Task<IdentityResultModel> LoginUserAsync(string userName, string password, bool rememberMe);
    }
}
using AVStack.IdentityServer.WebApi.Models.Application.Interfaces;
using AVStack.IdentityServer.WebApi.Models.System;
using System;
using System.Threading.Tasks;

namespace AVStack.IdentityServer.WebApi.Services.Interfaces
{
    public interface IAccountService
    {
        Task<IdentityResultModel> ConfirmEmailAsync(Guid userId, string token);
        Task<IdentityResultModel> ForgotPasswordAsync(string email);
        Task<IdentityResultModel> LoginUserAsync(string userName, string password, bool rememberMe);
        Task LogoutUserAsync();
        Task<IdentityResultModel> RegisterUserAsync(IUser user, string role = null);
        Task<IdentityResultModel> ResetPasswordAsync(Guid userId, string password, string token);
    }
}
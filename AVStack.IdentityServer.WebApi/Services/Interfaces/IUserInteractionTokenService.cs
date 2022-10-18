using System.Threading.Tasks;
using AVStack.IdentityServer.Common.Enums;
using AVStack.IdentityServer.WebApi.Data.Entities;

namespace AVStack.IdentityServer.WebApi.Services.Interfaces
{
    public interface IUserInteractionTokenService
    {
        Task<string> CreateCallbackByEventType(EventType eventType, UserEntity entity);
    }
}
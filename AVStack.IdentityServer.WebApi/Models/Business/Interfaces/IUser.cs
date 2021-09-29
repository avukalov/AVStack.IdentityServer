using System;

namespace AVStack.IdentityServer.WebApi.Models.Business.Interfaces
{
    public interface IUser
    {
        Guid Id { get; set; }
        string FirstName { get; set; }
        string LastName { get; set; }
        string UserName { get; set; }
        string Email { get; set; }
        // Guid AccountId { get; set; }
    }
}
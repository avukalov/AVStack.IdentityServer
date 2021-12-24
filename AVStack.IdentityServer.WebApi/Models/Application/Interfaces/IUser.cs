using System;

namespace AVStack.IdentityServer.WebApi.Models.Application.Interfaces
{
    public interface IUser
    {
        Guid Id { get; set; }
        string FirstName { get; set; }
        string LastName { get; set; }
        string UserName { get; set; }
        string Email { get; set; }
        string Password { get; set; }
        string FullName => $"{FirstName} {LastName}";

        // Guid AccountId { get; set; }
    }
}
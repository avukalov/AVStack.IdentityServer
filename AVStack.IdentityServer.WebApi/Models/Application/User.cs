using System;
using AVStack.IdentityServer.WebApi.Models.Application.Interfaces;

namespace AVStack.IdentityServer.WebApi.Models.Application
{
    public class User : IUser
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }

        public string FullName => $"{FirstName} {LastName}";
        // public Guid AccountId { get; set; }
    }
}
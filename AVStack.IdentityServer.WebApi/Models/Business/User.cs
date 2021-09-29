using System;
using AVStack.IdentityServer.WebApi.Models.Business.Interfaces;

namespace AVStack.IdentityServer.WebApi.Models.Business
{
    public class User : IUser
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        // public Guid AccountId { get; set; }
    }
}
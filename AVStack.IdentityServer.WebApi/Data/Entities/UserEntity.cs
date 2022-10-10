using System;
using Microsoft.AspNetCore.Identity;

namespace AVStack.IdentityServer.WebApi.Data.Entities
{
    public class UserEntity : IdentityUser<Guid>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string FullName => $"{FirstName} {LastName}";
        
        public DateTime DateCreated { get; set; }
    }
}
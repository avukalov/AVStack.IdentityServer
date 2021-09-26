using System;
using AVStack.IdentityServer.Constants;
using Microsoft.AspNetCore.Identity;

namespace AVStack.IdentityServer.WebApi.Data.Entities
{
    public class RoleEntity : IdentityRole<Guid>
    {
        public RoleLevel Level { get; set; }
    }
}
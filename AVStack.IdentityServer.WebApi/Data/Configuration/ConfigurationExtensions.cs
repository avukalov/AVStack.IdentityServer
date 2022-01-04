using System;
using AVStack.IdentityServer.WebApi.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AVStack.IdentityServer.WebApi.Data.Configuration
{
    public static class ConfigurationExtensions
    {
        public static void ConfigureEntities(this ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new UserEntityConfiguration());
            modelBuilder.ApplyConfiguration(new UserTempTokenConfiguration());

            modelBuilder.Entity<IdentityUserClaim<Guid>>().ToTable("AVUserClaim");
            modelBuilder.Entity<IdentityUserRole<Guid>>().ToTable("AVUserRole");
            modelBuilder.Entity<IdentityUserLogin<Guid>>().ToTable("AVUserLogin");
            modelBuilder.Entity<IdentityUserToken<Guid>>().ToTable("AVUserToken");

            modelBuilder.Entity<RoleEntity>().ToTable("AVRole");
            modelBuilder.Entity<IdentityRoleClaim<Guid>>().ToTable("AVRoleClaim");
        }
    }
}
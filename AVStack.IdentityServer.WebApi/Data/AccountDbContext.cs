using System;
using AVStack.IdentityServer.WebApi.Data.Configuration;
using AVStack.IdentityServer.WebApi.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AVStack.IdentityServer.WebApi.Data
{
    public class AccountDbContext : IdentityDbContext<UserEntity, RoleEntity, Guid>
    {
        public DbSet<UserLoginHistory> UserLoginHistory { get; set; }
        
        public AccountDbContext(DbContextOptions<AccountDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new UserEntityConfiguration());

            modelBuilder.Entity<IdentityUserClaim<Guid>>().ToTable("AVUserClaim");
            modelBuilder.Entity<IdentityUserRole<Guid>>().ToTable("AVUserRole");
            modelBuilder.Entity<IdentityUserLogin<Guid>>().ToTable("AVUserLogin");
            modelBuilder.Entity<IdentityUserToken<Guid>>().ToTable("AVUserToken");

            modelBuilder.Entity<RoleEntity>().ToTable("AVRole");
            modelBuilder.Entity<IdentityRoleClaim<Guid>>().ToTable("AVRoleClaim");

            modelBuilder.Entity<UserLoginHistory>().ToTable("AVUserLoginHistory");
        }
    }
}
using AVStack.IdentityServer.WebApi.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AVStack.IdentityServer.WebApi.Data.Configuration
{
    public class UserEntityConfiguration : IEntityTypeConfiguration<UserEntity>
    {
        public void Configure(EntityTypeBuilder<UserEntity> builder)
        {
            builder.ToTable("User");
            
            builder.Property(u => u.FirstName).HasMaxLength(100).IsRequired();
            builder.Property(u => u.LastName).HasMaxLength(100).IsRequired();
            builder.Property(u => u.UserName).HasMaxLength(100).IsRequired();
            builder.Property(u => u.Email).HasMaxLength(100).IsRequired();
            
            builder.Property(u => u.NormalizedUserName).HasMaxLength(100);
            builder.Property(u => u.NormalizedEmail).HasMaxLength(100);
            builder.Property(u => u.PhoneNumber).HasMaxLength(30);
        }
    }
}
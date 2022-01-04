using AVStack.IdentityServer.WebApi.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AVStack.IdentityServer.WebApi.Data.Configuration
{
    public class UserTempTokenConfiguration : IEntityTypeConfiguration<UserTempToken>
    {
        public void Configure(EntityTypeBuilder<UserTempToken> builder)
        {
            builder.ToTable("AVUserTempToken");

            builder.HasKey(p => p.Id);

            builder
                .Property(p => p.Value)
                .HasColumnType("TEXT")
                .HasMaxLength(int.MaxValue)
                .IsRequired();

            builder
                .Property(p => p.TokenType)
                .HasMaxLength(50)
                .IsRequired();

        }
    }
}
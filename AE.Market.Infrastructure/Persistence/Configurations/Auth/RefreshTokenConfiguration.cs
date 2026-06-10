using AE.Market.Domain.Aggregates.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AE.Market.Infrastructure.Persistence.Configurations.Auth
{
    internal sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.ToTable("refresh_tokens", "auth").HasKey(t => t.Id);
            builder.HasIndex(t => t.TokenHash);
            builder.HasIndex(t => t.UserId);

            builder.Property(t => t.TokenHash).HasMaxLength(64).IsRequired();

            builder.HasOne<User>()
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
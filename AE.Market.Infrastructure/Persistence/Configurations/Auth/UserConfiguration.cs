using AE.Market.Domain.Aggregates.Auth;
using AE.Market.Domain.Aggregates.Auth.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AE.Market.Infrastructure.Persistence.Configurations.Auth
{
    internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("users","auth");
            builder.HasKey(t => t.Id);
            builder.HasIndex(t => t.Email);
            builder.Property(u => u.Email)
                .HasConversion(v => v.Value, v => EmailAddress.Create(v).Value)
                .HasMaxLength(200)
                .IsRequired();
            builder.Property(u => u.PasswordHash)
                .HasConversion(v => v.Value, v => PasswordHash.FromHashedString(v).Value);
        }
    }
}

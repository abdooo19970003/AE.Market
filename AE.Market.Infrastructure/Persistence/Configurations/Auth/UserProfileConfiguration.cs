using AE.Market.Domain.Aggregates.Auth;
using AE.Market.Domain.Aggregates.Auth.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AE.Market.Infrastructure.Persistence.Configurations.Auth
{
    internal sealed class UserProfileConfiguration : IEntityTypeConfiguration<UserProfile>
    {
        public void Configure(EntityTypeBuilder<UserProfile> builder)
        {
            builder.ToTable("user_profiles", "auth")
                .HasKey(x => x.Id);
            builder.HasIndex(x => x.FirstName);

            builder.HasOne<User>()
                .WithOne(u => u.Profile)
                .HasForeignKey<UserProfile>(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Property(x => x.FirstName)
                .HasConversion(v => v == null ? null : v.Value, v => v == null ? null : (Name)v)
                .HasMaxLength(50);
            builder.Property(x => x.LastName)
                .HasConversion(v => v == null ? null : v.Value, v => v == null ? null : (Name)v)
                .HasMaxLength(50);
            builder.Property(x => x.Phone)
                .HasConversion(v => v == null ? null : v.Value, v => v == null ? null : (PhoneNumber)v)
                .HasMaxLength(50);
            builder.Property(x => x.ProfileImage)
                .HasConversion(v => v == null ? null : v.Value, v => v == null ? null : (ImageUrl)v)
                .HasMaxLength(1000);
            builder.ComplexProperty(x => x.Address).HasDiscriminator();
                
        }
    }
}

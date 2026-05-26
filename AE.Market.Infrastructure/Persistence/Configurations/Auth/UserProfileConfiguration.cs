using AE.Market.Domain.Aggregates.Auth;
using AE.Market.Domain.Aggregates.Auth.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AE.Market.Infrastructure.Persistence.Configurations.Auth
{
    internal class UserProfileConfiguration : IEntityTypeConfiguration<UserProfile>
    {
        public void Configure(EntityTypeBuilder<UserProfile> builder)
        {
            builder.ToTable("user_profiles", "auth")
                .HasKey(x => x.Id);
            builder.HasIndex(x => x.FirstName);

            builder
                .HasOne(p => p.User)
                .WithOne(u => u.Profile)
                .HasForeignKey<UserProfile>(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Property(x => x.FirstName)
                .HasConversion(v => v.Value, v => (Name)v)
                .HasMaxLength(50);
            builder.Property(x => x.LastName)
                .HasConversion(v => v.Value, v => (Name)v)
                .HasMaxLength(50);
            builder.Property(x => x.Phone)
                .HasConversion(v => v.Value, v => (PhoneNumber)v)
                .HasMaxLength(50);
            builder.Property(x => x.ProfileImage)
                .HasConversion(v => v.Value, v => (ImageUrl)v)
                .HasMaxLength(1000);
            builder.ComplexProperty(x => x.Address).HasDiscriminator();
                
        }
    }
}

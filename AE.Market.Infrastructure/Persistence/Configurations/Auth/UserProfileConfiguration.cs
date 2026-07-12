using AE.Market.Domain.Aggregates.Auth;
using AE.Market.Domain.Aggregates.Auth.ValueObjects;
using AE.Market.Domain.Common.Enums;
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
                .HasConversion(v => v == null ? null : v.Value, v => v == null ? null : (Domain.Aggregates.Auth.ValueObjects.Name)v)
                .HasMaxLength(50);
            builder.Property(x => x.LastName)
                .HasConversion(v => v == null ? null : v.Value, v => v == null ? null : (Domain.Aggregates.Auth.ValueObjects.Name)v)
                .HasMaxLength(50);
            builder.Property(x => x.Phone)
                .HasConversion(v => v == null ? null : v.Value, v => v == null ? null : (PhoneNumber)v)
                .HasMaxLength(50);
            builder.Property(x => x.ProfileImage)
                .HasConversion(v => v == null ? null : v.Value, v => v == null ? null : (ImageUrl)v)
                .HasMaxLength(1000);

            builder.OwnsMany(x => x.Addresses, a =>
            {
                a.ToTable("user_addresses", "auth");
                a.WithOwner().HasForeignKey("UserProfileId");
                a.Property<int>("Id").ValueGeneratedOnAdd();
                a.HasKey("Id");
                a.Property(addr => addr.Country).HasMaxLength(100).IsRequired();
                a.Property(addr => addr.City).HasMaxLength(100).IsRequired();
                a.Property(addr => addr.ZipCode).HasMaxLength(20);
                a.Property(addr => addr.AddressLine).HasMaxLength(500);
                a.Property(addr => addr.Label).HasMaxLength(100);
                a.Property(addr => addr.Type).HasConversion<string>().HasMaxLength(50);
                a.Property(addr => addr.IsPrimary).HasDefaultValue(false);
            });
        }
    }
}

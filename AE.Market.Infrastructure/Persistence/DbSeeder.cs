using AE.Market.Application.Services;
using AE.Market.Domain.Aggregates.Auth;
using Microsoft.EntityFrameworkCore;

namespace AE.Market.Infrastructure.Persistence;

public sealed class DbSeeder(AppDbContext db, IPasswordService passwordService)
{
    public async Task SeedAsync()
    {
        if (await db.Users.AnyAsync())
            return;

        // Admin user — has all permissions
        var adminPassword = passwordService.HashPassword("Admin@12345");
        var admin = User.Register(Guid.NewGuid(), "admin@aemarket.com", adminPassword);
        admin.AddPermission(Permission.AccessUsers);
        admin.AddPermission(Permission.MutateUsers);
        db.Users.Add(admin);

        // Client user — no special permissions
        var clientPassword = passwordService.HashPassword("Client@12345");
        var client = User.Register(Guid.NewGuid(), "client@aemarket.com", clientPassword);
        db.Users.Add(client);

        await db.SaveChangesAsync();
    }
}
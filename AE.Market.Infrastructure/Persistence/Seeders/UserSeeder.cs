using AE.Market.Application.Services;
using AE.Market.Domain.Aggregates.Auth;

namespace AE.Market.Infrastructure.Persistence.Seeders;

public static class UserSeeder
{
    public static List<User> GetSeedData(IPasswordService passwordService)
    {
        var adminPassword = passwordService.HashPassword("Admin@12345");
        var admin = User.Register(Guid.NewGuid(), "admin@aemarket.com", adminPassword);
        admin.AddPermission(Permission.AccessUsers);
        admin.AddPermission(Permission.MutateUsers);
        admin.AddPermission(Permission.MutateCategories);
        admin.AddPermission(Permission.MutateProducts);
        admin.AddPermission(Permission.MutateUnits);
        admin.AddPermission(Permission.MutateTaxCodes);
        admin.AddPermission(Permission.MutateBrands);

        var clientPassword = passwordService.HashPassword("Client@12345");
        var client = User.Register(Guid.NewGuid(), "client@aemarket.com", clientPassword);

        return [admin, client];
    }
}

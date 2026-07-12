using AE.Market.Domain.Aggregates.Catalog.Units;

namespace AE.Market.Infrastructure.Persistence.Seeders;

public static class GroupUnitSeeder
{
    public static List<GroupUnit> GetSeedData()
    {
        var weight = GroupUnit.Create(Guid.NewGuid(), "Weight");
        weight.AddUnit(Guid.NewGuid(), "Kilogram", "kg", true);
        weight.AddUnit(Guid.NewGuid(), "Gram", "g", false, 0.001m);
        weight.AddUnit(Guid.NewGuid(), "Pound", "lb", false, 0.453592m);

        var length = GroupUnit.Create(Guid.NewGuid(), "Length");
        length.AddUnit(Guid.NewGuid(), "Centimeter", "cm", true);
        length.AddUnit(Guid.NewGuid(), "Meter", "m", false, 100m);
        length.AddUnit(Guid.NewGuid(), "Inch", "in", false, 2.54m);

        return [weight, length];
    }
}

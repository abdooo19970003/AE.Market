using System.Reflection;
using AE.Market.Application;
using FluentAssertions;
using NetArchTest.Rules;

namespace AE.Market.ArchitectureTests.Application;

public class FeatureAggregateIsolationTests : BaseTest
{
    private static readonly Assembly Assembly = typeof(DependencyInjection).Assembly;

    private static readonly Dictionary<string, string[]> FeatureAggregateMap = new()
    {
        ["Auth"] = ["AE.Market.Domain.Aggregates.Auth"],
        ["Categories"] = ["AE.Market.Domain.Aggregates.Catalog"],
        ["Products"] = ["AE.Market.Domain.Aggregates.Catalog"],
        ["Variants"] = ["AE.Market.Domain.Aggregates.Catalog"],
        ["Attributes"] = ["AE.Market.Domain.Aggregates.Catalog"],
        ["Pricing"] = ["AE.Market.Domain.Aggregates.Prices", "AE.Market.Domain.Aggregates.Catalog"],
        ["Inventory"] = ["AE.Market.Domain.Aggregates.Inventory", "AE.Market.Domain.Aggregates.Catalog"],
        ["Cart"] = ["AE.Market.Domain.Aggregates.Cart"],
        ["Orders"] = ["AE.Market.Domain.Aggregates.Orders"],
    };

    private static readonly string[] AllAggregates =
        FeatureAggregateMap.Values.SelectMany(v => v).Distinct().ToArray();

    public static IEnumerable<object[]> GetRestrictedFeatures()
    {
        return FeatureAggregateMap.Keys.Select(f => new object[] { f });
    }

    [Theory]
    [MemberData(nameof(GetRestrictedFeatures))]
    public void Feature_ShouldNotDependOn_OtherAggregates(string feature)
    {
        var allowed = FeatureAggregateMap[feature];
        var disallowed = AllAggregates.Except(allowed).ToList();

        if (disallowed.Count == 0)
            return;

        var chain = Types
            .InAssembly(Assembly)
            .That()
            .ResideInNamespace($"AE.Market.Application.Features.{feature}")
            .Should()
            .NotHaveDependencyOn(disallowed[0]);

        for (int i = 1; i < disallowed.Count; i++)
            chain = chain.And().NotHaveDependencyOn(disallowed[i]);

        var result = chain.GetResult();
        result.IsSuccessful.Should().BeTrue(
            $"because '{feature}' must only reference its own aggregate(s): [{string.Join(", ", allowed)}]. " +
            $"Violations: [{string.Join(", ", result.FailingTypeNames ?? [])}]");
    }
}

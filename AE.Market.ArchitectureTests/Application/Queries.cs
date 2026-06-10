using AE.Market.Application.Common.Abstracts;
using AE.Market.Application.Common.Interfaces;
using FluentAssertions;
using NetArchTest.Rules;

namespace AE.Market.ArchitectureTests.Application
{
    public class Queries : BaseTest
    {
        [Fact]
        public void Queries_Should_BeSealed_And_EndWithQuery()
        {
            var result = Types
                .InAssembly(ApplicationAssembly)
                .That()
                .AreMutable()
                .And()
                .ImplementInterface(BaseQueryType)
                .Should()
                .HaveNameEndingWith("Query")
                .And()
                .BeSealed()
                .GetResult();

            result
                .IsSuccessful.Should()
                .BeTrue(
                    "because all CQRS Queries should be sealed to prevent inheritance and its name should end with Query."
                );
        }

        [Fact]
        public void CachedQueries_Should_Have_AbsoluteExpiration_Always()
        {
            var cachedQueryTypes = Types
                .InAssembly(ApplicationAssembly)
                .That()
                .AreClasses()
                .And()
                .ImplementInterface(typeof(ICachedQuery))
                .GetTypes();

            var failingTypes = new List<string>();

            foreach (var type in cachedQueryTypes)
            {
                try
                {
                    var instance = Activator.CreateInstance(type);
                    var property = type.GetProperty("AbsoluteExpiration");
                    if (property != null)
                    {
                        var value = property.GetValue(instance);
                        if (value is null)
                            failingTypes.Add(type.Name);
                    }
                }
                catch (MissingMethodException)
                {
                    // in case query does not have parameterless ctor
                    var property = type.GetProperty("AbsoluteExpiration");
                    if (
                        property != null
                        && Nullable.GetUnderlyingType(property.PropertyType) != null
                    )
                    {
                        failingTypes.Add(
                            $"{type.FullName} (Requires constructor arguments, but uses a nullable TimeSpan?)"
                        );
                    }
                }
            }

            // Assertion
            failingTypes
                .Should()
                .BeEmpty(
                    "because all classes implementing ICachedQuery must provide a valid, non-null AbsoluteExpiration value "
                        + "to prevent data from being cached indefinitely."
                );
        }


        [Fact]
        public void QueryHandlers_Should_NotInjectWritRepository()
        {
            var results = Types
                .InAssembly(ApplicationAssembly)
                .That()
                .ImplementInterface(BaseQueryType)
                .Should()
                .NotHaveDependencyOn(typeof(IRepository<>).FullName)
                .GetResult();
            results
                .IsSuccessful.Should()
                .BeTrue("because query handler should not inject write repository.");
        }

    }
}

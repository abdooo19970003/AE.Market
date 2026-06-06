using FluentAssertions;
using NetArchTest.Rules;

namespace AE.Market.ArchitectureTests.Infrastructure
{
    public class InfrastructureTests:BaseTest
    {
        [Fact]
        public void InfrastructureImplementations_Should_BeSealed()
        {
            var result = Types.InAssembly(InfrastructureAssembly)
                .That()
                .HaveNameEndingWith("Repository")
                .Or()
                .HaveNameEndingWith("Service")
                .Should()
                .BeSealed()
                .GetResult();

            result.IsSuccessful.Should().BeTrue("because external infrastructure implementations should be sealed.");
        }

        [Fact] 
        public void TypeConfiguration_Should_BeSealed()
        {
            var result = Types
                .InAssembly(InfrastructureAssembly)
                .That().HaveNameEndingWith("Configuration")
                .Should()
                .BeSealed()
                .GetResult();

            result.IsSuccessful.Should().BeTrue("because configurations should be sealed to prevent inheritance.");
        }
    }
}

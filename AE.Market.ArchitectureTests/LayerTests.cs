using FluentAssertions;
using NetArchTest.Rules;

namespace AE.Market.ArchitectureTests
{
    public class LayerTests : BaseTest
    {
        [Fact]
        public void Domain_Should_NotHaveDependencyOnApplication()
        {
            var result = Types
                .InAssembly(DomainAssembly)
                .Should()
                .NotHaveDependencyOn("AE.Market.Application")
                .GetResult();
            result.IsSuccessful.Should().BeTrue();
        }
        [Fact]
        public void Domain_Should_NotHaveDependencyOnInfrastructure()
        {
            var result = Types
                .InAssembly(DomainAssembly)
                .Should()
                .NotHaveDependencyOn("AE.Market.Infrastructure")
                .GetResult();
            result.IsSuccessful.Should().BeTrue();
        }
        [Fact]
        public void Application_Should_NotHaveDependencyOnInfrastructure()
        {
            var result = Types
                .InAssembly(ApplicationAssembly)
                .Should()
                .NotHaveDependencyOn("AE.Market.Infrastructure")
                .GetResult();
            result.IsSuccessful.Should().BeTrue();
        }

        [Fact]
        public void DomainAndApplication_Should_NotHaveDependencyOnPresentation()
        {
            var result = Types.InAssemblies(new[] { DomainAssembly, ApplicationAssembly })
                .Should()
                .NotHaveDependencyOn("AE.Market.API") // Or whatever your API project namespace is
                .GetResult();

            result.IsSuccessful.Should().BeTrue("because the inner core must remain completely decoupled from the Web/API layer.");
        }

        [Fact]
        public void Application_Should_HaveDependencyOnDomain()
        {
            // Arrange
            var expectedDependency = "AE.Market.Domain";

            // Act
            var referencedAssemblies = ApplicationAssembly.GetReferencedAssemblies();
            bool hasDependency = referencedAssemblies.Any(a => a.Name == expectedDependency);

            // Assert
            Assert.True(hasDependency, $"The Application layer must reference {expectedDependency}.");
        }
    }
}

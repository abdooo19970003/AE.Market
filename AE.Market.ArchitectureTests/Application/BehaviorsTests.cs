using AE.Market.Application;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using NetArchTest.Rules;

namespace AE.Market.ArchitectureTests.Application
{
    public class BehaviorsTests : BaseTest
    {
        [Fact]
        public void PiplineBehviors_Should_BeSealed_And_EndWithBeahavior()
        {
            var result = Types
                .InAssembly(ApplicationAssembly)
                .That()
                .ImplementInterface(typeof(IPipelineBehavior<,>))
                .Should()
                .BeSealed()
                .And()
                .HaveNameMatching("Behavior(`\\d+)?$")
                .GetResult();
            var failingTypes = string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>());

            result
                .IsSuccessful.Should()
                .BeTrue(
                    $"because all MediatR pipeline behaviors must be sealed and end with the suffix 'Behavior'. "
                        + $"Failing types: [{failingTypes}]"
                );
        }

        [Fact]
        public void PipelineBehaviors_Should_BeRegisteredInCorrectOrder()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddApplicationServices();

            var expectedOrder = new[]
            {
                "ExceptionHandlerBehavior",
                "LoggingBehavior",
                "CachingBehavior",
                "ValidationBehavior",
                "TransactionBehavior",
            };

            // Act
            var registerdBehaviors = services
                .Where(d => d.ServiceType == typeof(IPipelineBehavior<,>))
                .Select(d => d.ImplementationType?.Name.Split("`")[0])
                .ToList();

            // Assert
            registerdBehaviors
                .Should()
                .ContainInOrder(
                    expectedOrder,
                    "because the pipeline must process errors and caching before opening transactions."
                );
        }
    }
}

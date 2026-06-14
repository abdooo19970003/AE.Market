using AE.Market.Application.Common.Interfaces;
using AE.Market.ArchitectureTests.CustomRules;
using AE.Market.Domain.Common.Abstracts;
using FluentAssertions;
using MediatR;
using NetArchTest.Rules;

namespace AE.Market.ArchitectureTests.Application
{
    public class ApplicationTests : BaseTest
    {
        [Fact]
        public void Handlers_Should_BeSealed_And_EndWithHandler()
        {
            var result = Types
                .InAssembly(ApplicationAssembly)
                .That()
                .AreClasses()
                .And()
                .ImplementInterface(HandlerType)
                .Should()
                .HaveNameEndingWith("Handler")
                .And()
                .BeSealed()
                .GetResult();

            result
                .IsSuccessful.Should()
                .BeTrue(
                    "because all CQRS command/query handlers should be sealed to prevent inheritance and its name should end with Handler."
                );
        }

        [Fact]
        public void Handlers_Should_ReturunResult()
        {
            var handlersTypes = Types
                .InAssembly(ApplicationAssembly)
                .That()
                .AreClasses()
                .And()
                .ImplementInterface(HandlerType)
                .GetTypes();

            List<string> failingTypes = new();

            foreach (Type handler in handlersTypes)
            {
                var mediatrInterface = handler
                    .GetInterfaces()
                    .FirstOrDefault(i =>
                        i.IsGenericType
                        && i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>)
                    );
                if (mediatrInterface != null)
                {
                    var responseType = mediatrInterface.GetGenericArguments()[1];

                    bool isResultType = responseType == typeof(Result);
                    bool isGenericResultType =
                        responseType.IsGenericType
                        && responseType.GetGenericTypeDefinition() == typeof(Result<>);
                    if (!isGenericResultType && !isResultType)
                        failingTypes.Add(handler.FullName ?? handler.Name);
                }
            }
            var failingTypesList = string.Join(",", failingTypes);

            failingTypes
                .Should()
                .BeEmpty(
                    $"because every MediatR handler must return a Result or Result<T> wrapper to handle errors cleanly. "
                        + $"Failing handlers: [{failingTypesList}]"
                );
        }

        [Fact]
        public void DTOs_Should_BeSealed_AndPublic()
        {
            var result = Types
                .InAssembly(ApplicationAssembly)
                .That()
                .HaveNameEndingWith("Dto")
                .Should()
                .BeSealed()
                .And()
                .BePublic()
                .GetResult();

            result.IsSuccessful.Should().BeTrue("because all DTOs should be sealed and public");
        }

        [Fact] 
        public void AsyncMethods_Should_HasCancellationTokenParameterAsLastParameter()
        {
            var result = Types
                .InAssembly(ApplicationAssembly)
                .That()
                .AreInterfaces()
                .Should()
                .MeetCustomRule(new CancellationTokenIsLastParameterRule())
                .GetResult();
            var failingInterfaces = string.Join(", ", result.FailingTypeNames ?? []);

            result.IsSuccessful.Should().BeTrue(
                $"because all asynchronous interface methods must accept a CancellationToken as their final parameter " +
                $"to ensure proper cooperative cancellation flows. Failing interfaces: [{failingInterfaces}]"
            );
        }
    }
}

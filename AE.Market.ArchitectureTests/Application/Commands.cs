using FluentAssertions;
using FluentValidation;
using NetArchTest.Rules;

namespace AE.Market.ArchitectureTests.Application
{
    public class Commands : BaseTest
    {
        [Fact]
        public void Commands_Should_BeSealed_And_EndWithCommand()
        {
            var result = Types
                .InAssembly(ApplicationAssembly)
                .That()
                .AreMutable()
                .And()
                .ImplementInterface(BaseCommandType)
                .Should()
                .HaveNameEndingWith("Command")
                .And()
                .BeSealed()
                .GetResult();

            result
                .IsSuccessful.Should()
                .BeTrue(
                    "because all CQRS commands should be sealed to prevent inheritance and its name should end with Command."
                );
        }

        [Fact]
        public void EveryCommand_Should_HaveCorrespondingValidator()
        {
            // 1. Get all concrete command types in the assembly
            var commandTypes = Types
                .InAssembly(ApplicationAssembly)
                .That()
                .AreClasses()
                .And()
                .ImplementInterface(BaseCommandType)
                .GetTypes();

            // 2. Use pure Reflection to safely discover FluentValidation types
            var validatedTypes = ApplicationAssembly.GetTypes()
                .Where(type => type.IsClass && !type.IsAbstract)
                .SelectMany(type => type.GetInterfaces())
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IValidator<>))
                .Select(i => i.GetGenericArguments()[0])
                .ToHashSet();

            // 3. Find which commands are missing from the validatedTypes set
            var failingCommands = commandTypes
                .Where(command => !validatedTypes.Contains(command))
                .Select(command => command.Name)
                .ToList();

            // 4. Assert using FluentAssertions
            var failingCommandsList = string.Join(", ", failingCommands);
            failingCommands.Should().BeEmpty(
                $"because every command must have a corresponding FluentValidation validator. " +
                $"Failing commands missing a validator: [{failingCommandsList}]"
            );
        }
    }
}

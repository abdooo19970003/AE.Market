using System.Reflection;
using AE.Market.ArchitectureTests.CustomRules;
using AE.Market.Domain.Common;
using FluentAssertions;
using NetArchTest.Rules;

namespace AE.Market.ArchitectureTests.Domain
{
    public class DomainTests : BaseTest
    {
        [Fact]
        public void DomainEvents_Should_Be_Sealed()
        {
            var result = Types
                .InAssembly(DomainAssembly)
                .That()
                .ImplementInterface(typeof(IDomainEvent))
                .Should()
                .BeSealed()
                .GetResult();
            result
                .IsSuccessful.Should()
                .BeTrue("because domain events should be immutable,sealed records/classes");
        }

        [Fact]
        public void DomainEvents_Should_HaveDomainEventsPostfix()
        {
            var result = Types
                .InAssembly(DomainAssembly)
                .That()
                .ImplementInterface(typeof(IDomainEvent))
                .Should()
                .HaveNameEndingWith("DomainEvent")
                .GetResult();

            result
                .IsSuccessful.Should()
                .BeTrue("because domain events name should end with DomainEvent");
        }

        [Fact]
        public void Entities_Should_HasParameterlessConstructor()
        {
            var EntitiesTypes = Types
                .InAssembly(DomainAssembly)
                .That()
                .Inherit(typeof(BaseEntity))
                .GetTypes();

            var failingTypes = new List<Type>();

            foreach (Type type in EntitiesTypes)
            {
                var constructors = type.GetConstructors(
                    BindingFlags.NonPublic | BindingFlags.Instance
                );

                if (!constructors.Any(c => c.IsPrivate && c.GetParameters().Length == 0))
                {
                    failingTypes.Add(type);
                }
            }
            failingTypes
                .Should()
                .BeEmpty(
                    "because entities should has paramaterless private construtor for ORM",
                    failingTypes
                );
        }

        [Fact]
        public void Entities_Should_BeSealedClasses()
        {
            var result = Types
                .InAssembly(DomainAssembly)
                .That()
                .Inherit(typeof(BaseEntity))
                .Should()
                .BeSealed()
                .GetResult();
            result.IsSuccessful.Should().BeTrue();
        }

        [Fact]
        public void NotAggregateRootEntities_Should_NotHasPublicMethod()
        {
            var NotAggregateRootEntities = Types
                .InAssembly(DomainAssembly)
                .That()
                .Inherit(typeof(BaseEntity))
                .And()
                .DoNotImplementInterface(typeof(IAggregateRoot))
                .GetTypes();
            List<Type> FailingTypes = new();

            foreach (Type entity in NotAggregateRootEntities)
            {
                var methods = entity.Methods();
                if (methods.Any(m => m.IsPublic))
                {
                    FailingTypes.Add(entity);
                }
            }

            FailingTypes.Should().BeEmpty();
        }

        [Fact]
        public void ValueObjects_Should_BeRecord()
        {
            var result = Types
                .InAssembly(DomainAssembly)
                .That()
                .ImplementInterface(typeof(IValueObject))
                .And()
                .AreNotAbstract()
                .Should()
                .MeetCustomRule(new BeRecord())
                .GetResult();
            var failingTypes = string.Join(",", result.FailingTypeNames ?? []);

            result
                .IsSuccessful.Should()
                .BeTrue(
                    $"because every concrete subclass of BaseEntity must provide its own overridden implementation of 'IdentityCheck'. "
                        + $"Failing types: [{failingTypes}]"
                );
        }

        [Fact]
        public void ValueObjects_Should_OverrideGetHashCode()
        {
            var result = Types
                .InAssembly(DomainAssembly)
                .That()
                .ImplementInterface(typeof(IValueObject))
                .And()
                .AreNotAbstract()
                .Should()
                .MeetCustomRule(new RequiredMethodOverrideRule("GetHashCode"))
                .GetResult();
            var failingTypes = string.Join(",", result.FailingTypeNames ?? []);

            result
                .IsSuccessful.Should()
                .BeTrue(
                    $"because every concrete of IValueObject must provide its own overridden implementation of 'GetHashCode'. "
                        + $"Failing types: [{failingTypes}]"
                );
        }
    }
}

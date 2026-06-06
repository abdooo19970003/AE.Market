using Mono.Cecil;
using NetArchTest.Rules;

namespace AE.Market.ArchitectureTests.CustomRules
{
    internal class BeRecord : ICustomRule
    {
        public bool MeetsRule(TypeDefinition type)
        {
            return type.Properties.Any(p => p.Name == "EqualityContract");
        }
    }
}

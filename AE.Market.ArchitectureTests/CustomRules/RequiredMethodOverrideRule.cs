using Mono.Cecil;
using NetArchTest.Rules;

namespace AE.Market.ArchitectureTests.CustomRules
{
    internal class RequiredMethodOverrideRule(string methodName) : ICustomRule
    {
        private readonly string _methodName = methodName;

        public bool MeetsRule(TypeDefinition type)
        {
            return type.Methods.Any(m => m.Name == _methodName);
        }
    }
}

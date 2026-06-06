using System;
using System.Collections.Generic;
using System.Text;
using Mono.Cecil;
using NetArchTest.Rules;

namespace AE.Market.ArchitectureTests.CustomRules
{
    internal class CancellationTokenIsLastParameterRule : ICustomRule
    {
        public bool MeetsRule(TypeDefinition type)
        {
            if (type.IsInterface)
            {
                foreach (var method in type.Methods)
                {
                    if (!IsAsyncMethod(method))
                        continue;
                    var parameters = method.Parameters;
                    if (!parameters.Any())
                        return false;
                    var lastParameter = parameters.Last();
                    bool IsCancellationToken =
                        lastParameter.ParameterType.Name == "CancellationToken"
                        || lastParameter.ParameterType.FullName
                            == "System.Threading.CancellationToken";
                    if (!IsCancellationToken)
                        return false;
                }
            }
            return true;
        }
        private static bool IsAsyncMethod(MethodDefinition method)
        {
            var returnTypeName = method.ReturnType.Name;
            return returnTypeName.StartsWith("Task") || returnTypeName.StartsWith("ValueTask");
        }
    }
}

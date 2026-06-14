using AE.Market.Application.Common.Abstracts;
using AE.Market.Domain.Common.Abstracts;
using MediatR;
using System.Reflection;

namespace AE.Market.ArchitectureTests
{
    public abstract class BaseTest
    {
        protected static readonly Assembly DomainAssembly = typeof(BaseEntity).Assembly!;
        protected static readonly Assembly ApplicationAssembly = typeof(Market.Application.DependencyInjection).Assembly!;
        protected static readonly Assembly InfrastructureAssembly = typeof(Market.Infrastructure.DependencyInjection).Assembly!;
        protected static readonly Assembly ApiAssembly = typeof(Market.API.Program).Assembly!;


        protected readonly Type BaseQueryType = typeof(IBaseQuery<>);
        protected readonly Type BaseCommandType = typeof(IBaseCommand);
        protected readonly Type HandlerType = typeof(IRequestHandler<,>);
    }
}

using AE.Market.Application.Common.Interfaces;
using AE.Market.Domain.Common;
using MediatR;
using System.Reflection;

namespace AE.Market.ArchitectureTests
{
    public abstract class BaseTest
    {
        protected static readonly Assembly DomainAssembly = typeof(BaseEntity).Assembly!;
        protected static readonly Assembly ApplicationAssembly = typeof(AE.Market.Application.DependencyInjection).Assembly!;
        protected static readonly Assembly InfrastructureAssembly = typeof(AE.Market.Infrastructure.DependencyInjection).Assembly!;


        protected readonly Type BaseQueryType = typeof(IBaseQuery<>);
        protected readonly Type BaseCommandType = typeof(IBaseCommand);
        protected readonly Type HandlerType = typeof(IRequestHandler<,>);
    }
}

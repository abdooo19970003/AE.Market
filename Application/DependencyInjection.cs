using AE.Market.Application.Common.Behaviors;
using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Common.Mapping;
using AE.Market.Application.Features.Cart.Services;
using AE.Market.Application.Features.Catalog.Services;
using AE.Market.Application.Features.Pricing.Services;
using AE.Market.Application.Services;
using AE.Market.Domain.Aggregates.Pricing;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace AE.Market.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ExceptionHandlerBehavior<,>));
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>) );
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>));
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(SearchBehavior<,>));
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));
            });

            services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly,includeInternalTypes:true);

            services.AddSingleton(MappingConfig.Configure());
            services.AddScoped<IMapper, AppMapper>();
            services.AddScoped<IRefreshTokenService, RefreshTokenService>();
            services.AddScoped<IPriceCalculator, PriceCalculatorService>();
            services.AddScoped<IStockManager, StockManager>();
            services.AddScoped<IProductVariantLookup, ProductVariantLookup>();
            services.AddScoped<ICartLookup, CartLookup>();

            return services;
        }
    }
}

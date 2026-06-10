using Mapster;

namespace AE.Market.Application.Common.Mapping;

internal sealed class AppMapper(TypeAdapterConfig config) : IMapper
{
    public TDestination Map<TDestination>(object source)
    {
        return source.Adapt<TDestination>(config);
    }
}

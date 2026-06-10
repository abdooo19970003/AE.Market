namespace AE.Market.Application.Common.Mapping;

public interface IMapper
{
    TDestination Map<TDestination>(object source);
}

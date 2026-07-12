using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Common.Mapping;
using AE.Market.Application.Features.Catalog.DTOs;
using AE.Market.Application.Features.Catalog.Specs;
using AE.Market.Domain.Aggregates.Catalog.Errors;
using AE.Market.Domain.Aggregates.Catalog.Products;
using AE.Market.Domain.Common.Abstracts;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Queries.Tags;

internal sealed class GetTagByIdQueryHandler(
    IReadRepository<Tag> repo,
    IMapper mapper
) : IRequestHandler<GetTagByIdQuery, Result<TagDto>>
{
    public async Task<Result<TagDto>> Handle(GetTagByIdQuery request, CancellationToken cancellationToken)
    {
        var tag = await repo.FirstOrDefaultAsync(new TagByIdSpec(request.Id), cancellationToken);
        if (tag is null)
            return Result<TagDto>.Fail(CatalogErrors.ProductNotFound);

        var dto = mapper.Map<TagDto>(tag);
        return Result<TagDto>.Success(dto);
    }
}

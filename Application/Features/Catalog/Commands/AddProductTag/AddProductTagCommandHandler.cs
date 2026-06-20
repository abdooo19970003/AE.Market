using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Common.Mapping;
using AE.Market.Application.Features.Catalog.DTOs;
using AE.Market.Domain.Aggregates.Catalog.Errors;
using AE.Market.Domain.Aggregates.Catalog.Products;
using AE.Market.Domain.Common.Abstracts;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Commands.AddProductTag;

internal sealed class AddProductTagCommandHandler(
    IRepository<Product> repo,
    IMapper mapper
) : IRequestHandler<AddProductTagCommand, Result<TagDto>>
{
    public async Task<Result<TagDto>> Handle(AddProductTagCommand request, CancellationToken cancellationToken)
    {
        var product = await repo.GetByIdWithTrackingAsync(request.ProductId, cancellationToken);
        if (product is null)
            return Result<TagDto>.Fail(CatalogErrors.ProductNotFound);

        product.AddTag(Guid.NewGuid(), request.Name, request.Slug);

        var tagDto = new TagDto { Name = request.Name, Slug = request.Slug };
        return Result<TagDto>.Success(tagDto);
    }
}

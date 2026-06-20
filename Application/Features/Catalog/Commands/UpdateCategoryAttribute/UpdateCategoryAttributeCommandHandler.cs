using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Common.Mapping;
using AE.Market.Application.Features.Catalog.DTOs;
using AE.Market.Domain.Aggregates.Catalog.Attributes;
using AE.Market.Domain.Aggregates.Catalog.Errors;
using AE.Market.Domain.Common.Abstracts;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Commands.UpdateCategoryAttribute;

internal sealed class UpdateCategoryAttributeCommandHandler(
    IRepository<CategoryAttribute> repo,
    IMapper mapper
) : IRequestHandler<UpdateCategoryAttributeCommand, Result<CategoryAttributeDto>>
{
    public async Task<Result<CategoryAttributeDto>> Handle(UpdateCategoryAttributeCommand request, CancellationToken cancellationToken)
    {
        var attribute = await repo.GetByIdWithTrackingAsync(request.Id, cancellationToken);
        if (attribute is null)
            return Result<CategoryAttributeDto>.Fail(CatalogErrors.AttributeNotFound);

        attribute.Update(request.AttributeName, request.IsRequired, request.IsFilterable, request.SortOrder);

        var dto = mapper.Map<CategoryAttributeDto>(attribute);
        return Result<CategoryAttributeDto>.Success(dto);
    }
}

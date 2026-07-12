using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Common.Mapping;
using AE.Market.Application.Features.Catalog.DTOs;
using AE.Market.Domain.Aggregates.Catalog.Attributes;
using AE.Market.Domain.Common.Abstracts;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Commands.CreateCategoryAttribute;

internal sealed class CreateCategoryAttributeCommandHandler(
    IRepository<CategoryAttribute> repo,
    IMapper mapper
) : IRequestHandler<CreateCategoryAttributeCommand, Result<CategoryAttributeDto>>
{
    public async Task<Result<CategoryAttributeDto>> Handle(CreateCategoryAttributeCommand request, CancellationToken cancellationToken)
    {
        var inputType = (AttributeInputType)Enum.Parse(typeof(AttributeInputType), request.InputType);

        var attribute = CategoryAttribute.Create(
            Guid.NewGuid(),
            request.AttributeName,
            inputType,
            request.CategoryId,
            request.IsRequired,
            request.IsFilterable,
            request.Slug,
            request.SortOrder,
            request.DefaultUnitId,
            request.AllowedGroupUnitId
        );

        await repo.AddAsync(attribute, cancellationToken);

        var dto = mapper.Map<CategoryAttributeDto>(attribute);
        return Result<CategoryAttributeDto>.Success(dto);
    }
}

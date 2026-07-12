using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Features.Catalog.DTOs;
using AE.Market.Domain.Aggregates.Catalog.Attributes;
using AE.Market.Domain.Aggregates.Catalog.Products;
using AE.Market.Domain.Common.Abstracts;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Queries.RequiredAttributes;

internal sealed class GetRequiredAttributesForProductQueryHandler(
    IReadRepository<CategoryAttribute> attributeRepo,
    IReadRepository<ProductAttributeValue> attributeValueRepo
) : IRequestHandler<GetRequiredAttributesForProductQuery, Result<List<RequiredAttributeDto>>>
{
    public async Task<Result<List<RequiredAttributeDto>>> Handle(
        GetRequiredAttributesForProductQuery request,
        CancellationToken cancellationToken)
    {
        var spec = new Domain.Common.Specifications.BaseSpecification<CategoryAttribute>(
            a => a.CategoryId == request.CategoryId
        );
        spec.AddInclude(a => a.Options);
        var attributes = await attributeRepo.ListWithSpecAsync(spec, cancellationToken);

        var existingValues = new List<ProductAttributeValue>();
        if (request.ProductId.HasValue)
        {
            var valueSpec = new Domain.Common.Specifications.BaseSpecification<ProductAttributeValue>(
                v => v.ProductId == request.ProductId.Value
            );
            existingValues = (await attributeValueRepo.ListWithSpecAsync(valueSpec, cancellationToken)).ToList();
        }

        var dtos = attributes.Select(attr =>
        {
            var existing = existingValues.FirstOrDefault(v => v.AttributeId == attr.Id);
            return new RequiredAttributeDto
            {
                AttributeId = attr.Id,
                AttributeName = attr.AttributeName,
                InputType = attr.InputType.ToString(),
                IsRequired = attr.IsRequired,
                IsVariantDefiner = existing?.IsVariantDefiner ?? false,
                CurrentValueId = existing?.Id,
                CurrentValue = FormatValue(existing)
            };
        }).ToList();

        return Result<List<RequiredAttributeDto>>.Success(dtos);
    }

    private static string? FormatValue(ProductAttributeValue? value)
    {
        if (value is null) return null;

        if (value.ValueOptionId.HasValue)
        {
            return value.ValueOptionId.Value.ToString();
        }
        return value.ValueText
            ?? value.ValueInteger?.ToString()
            ?? value.ValueDecimal?.ToString()
            ?? value.ValueBoolean?.ToString()
            ?? value.ValueDateTime?.ToString("O");
    }
}

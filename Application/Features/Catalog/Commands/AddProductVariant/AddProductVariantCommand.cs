using AE.Market.Application.Common.Abstracts;
using AE.Market.Application.Features.Catalog.DTOs;

namespace AE.Market.Application.Features.Catalog.Commands.AddProductVariant;

public sealed record AddProductVariantCommand(
    Guid ProductId,
    string Name,
    string Sku,
    IReadOnlyCollection<AttributeValueDto>? AttributeValues = null
) : ICommand<VariantDto>;

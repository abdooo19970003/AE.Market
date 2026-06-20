using AE.Market.Application.Common.Abstracts;
using AE.Market.Application.Features.Catalog.DTOs;

namespace AE.Market.Application.Features.Catalog.Commands.AddProductRelation;

public sealed record AddProductRelationCommand(
    Guid ProductId,
    Guid RelatedProductId,
    string Type,
    int SortOrder
) : ICommand<ProductRelationDto>;

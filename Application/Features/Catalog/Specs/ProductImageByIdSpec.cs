using AE.Market.Domain.Aggregates.Catalog.Products;
using AE.Market.Domain.Common.Specifications;

namespace AE.Market.Application.Features.Catalog.Specs;

public sealed class ProductImageByIdSpec(Guid id) : BaseSpecification<ProductImage>(i => i.Id == id);

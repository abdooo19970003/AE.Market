using AE.Market.Domain.Aggregates.Catalog.Products;
using AE.Market.Domain.Common.Specifications;

namespace AE.Market.Application.Features.Catalog.Specs;

public sealed class TagByIdSpec(Guid id) : BaseSpecification<Tag>(t => t.Id == id);

using AE.Market.Domain.Aggregates.Catalog.Attributes;
using AE.Market.Domain.Common.Specifications;

namespace AE.Market.Application.Features.Catalog.Specs;

public sealed class CategoryAttributesByCategorySpec(Guid categoryId) : BaseSpecification<CategoryAttribute>(a => a.CategoryId == categoryId);

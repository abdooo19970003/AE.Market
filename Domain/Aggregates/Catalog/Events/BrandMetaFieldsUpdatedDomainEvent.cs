using AE.Market.Domain.Common.Abstracts;

namespace AE.Market.Domain.Aggregates.Catalog.Events;

public sealed record BrandMetaFieldsUpdatedDomainEvent(Guid BrandId, string? MetaTitle, string? MetaDescription, string? MetaKeywords) : IDomainEvent;

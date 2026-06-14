using AE.Market.Domain.Common.Abstracts;

namespace AE.Market.Domain.Aggregates.Catalog.Events;

public sealed record ProductMetaFieldsUpdatedDomainEvent(Guid ProductId, string? MetaTitle, string? MetaDescription, string? MetaKeywords) : IDomainEvent;

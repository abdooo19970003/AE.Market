using AE.Market.Domain.Common.Abstracts;

namespace AE.Market.Domain.Aggregates.Catalog.Events;

public sealed record CategoryMetaFieldsUpdatedDomainEvent(Guid CategoryId, string? MetaTitle, string? MetaDescription, string? MetaKeywords) : IDomainEvent;

using AE.Market.Domain.Common.Abstracts;

namespace AE.Market.Domain.Aggregates.Catalog.Events;

public sealed record BrandDetailsUpdatedDomainEvent(Guid BrandId, string Name, string? ShortDescription, string? LongDescription, string? LogoUrl, string? WebsiteUrl, int SortOrder) : IDomainEvent;

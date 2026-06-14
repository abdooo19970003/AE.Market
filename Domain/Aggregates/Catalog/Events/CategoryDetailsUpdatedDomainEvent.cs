using AE.Market.Domain.Common.Abstracts;

namespace AE.Market.Domain.Aggregates.Catalog.Events;

public sealed record CategoryDetailsUpdatedDomainEvent(Guid CategoryId, string Name, string Description, string? ImageUrl, int SortOrder) : IDomainEvent;

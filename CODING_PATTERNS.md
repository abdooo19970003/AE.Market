# AE.Market Coding Patterns Reference

## Project Structure (per feature/sprint)
```
Domain/Aggregates/{Feature}/
├── {Entity}.cs              # aggregate root: sealed class : BaseEntity, IAggregateRoot
├── {ChildEntity}.cs         # child entity: sealed class : BaseEntity (no IAggregateRoot)
├── {Enum}.cs                # simple enum file
├── Errors/{Feature}Errors.cs
├── Events/{Action}DomainEvent.cs

Application/Features/{Feature}/
├── DTOs/{Entity}Dto.cs      # sealed record with { get; set; }
├── Specs/{Feature}Specs.cs  # BaseSpecification<T> sealed classes
├── CacheKeys.cs             # internal static class
├── Events/{Action}Handler.cs
├── Commands/{Action}/
│   ├── {Action}Command.cs
│   ├── {Action}CommandHandler.cs
│   └── {Action}CommandValidator.cs
└── Queries/{Action}/
    ├── {Action}Query.cs
    └── {Action}QueryHandler.cs

Infrastructure/Persistence/Configurations/{Schema}/
├── {Entity}Configuration.cs

API/Controllers/{Feature}Controller.cs

tests/Domain.Tests/Aggregates/{Feature}/{Entity}Tests.cs
tests/Integration.Tests/{Feature}IntegrationTests.cs
```

## Entity Pattern (Aggregate Root)
```csharp
using AE.Market.Domain.Common.Abstracts;
using AE.Market.Domain.Exceptions;
using AE.Market.Domain.Aggregates.{Feature}.Errors;
using AE.Market.Domain.Aggregates.{Feature}.Events;

namespace AE.Market.Domain.Aggregates.{Feature};

public sealed class {Entity} : BaseEntity, IAggregateRoot
{
    public Guid Property { get; private set; }

    // Private ORM ctor
    private {Entity}() { }

    // Private ctor with params — used by static factory
    private {Entity}(Guid id, ...) : base(id) { ... }

    // STATIC FACTORY — only way to create
    public static {Entity} Create(Guid id, ...)
    {
        // validate, throw DomainException or ArgumentException
        var entity = new {Entity}(id, ...);
        entity.AddDomainEvent(new {Action}DomainEvent(...));
        return entity;
    }

    // Domain methods — mutate + UpdateLastModified() + AddDomainEvent()
    public void DoSomething(...)
    {
        // guard/invariant checks → throw DomainException
        UpdateLastModified();
        AddDomainEvent(new ...DomainEvent(...));
    }
}
```

## Entity Child Pattern (no IAggregateRoot, no events)
```csharp
public sealed class {Child} : BaseEntity
{
    private {Child}() { }
    private {Child}(Guid id, ...) : base(id) { ... }
    public static {Child} Create(Guid id, ...) => new(id, ...);
}
```

## Domain Event
```csharp
using AE.Market.Domain.Common.Abstracts;

namespace AE.Market.Domain.Aggregates.{Feature}.Events;

public sealed record {Action}DomainEvent(Guid Param1, Guid Param2) : IDomainEvent;
```

## Error
```csharp
using AE.Market.Domain.Common.Abstracts;

namespace AE.Market.Domain.Aggregates.{Feature}.Errors;

public static class {Feature}Errors
{
    public static readonly Error {ErrorName} = new(
        "{Feature}.{Entity}.{ErrorType}",
        "Human readable message."
    );
}
```
Error code convention for HTTP mapping: code `.Contains("NotFound")` → 404, `Contains("AlreadyExist")` → 409

## Command (ICommand — no response)
```csharp
using AE.Market.Application.Common.Abstracts;

namespace AE.Market.Application.Features.{Feature}.Commands.{Action};

public sealed record {Action}Command(
    Guid Param1
) : ICommand;
```

## Command (ICommand<T> — with response)
```csharp
using AE.Market.Application.Common.Abstracts;
using AE.Market.Application.Features.{Feature}.DTOs;

namespace AE.Market.Application.Features.{Feature}.Commands.{Action};

public sealed record {Action}Command(
    Guid Param1
) : ICommand<{Entity}Dto>;
```

## Command Handler (with IRepository)
```csharp
using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Features.{Feature}.DTOs;
using AE.Market.Application.Features.{Feature}.Specs;
using AE.Market.Domain.Aggregates.{Feature};
using AE.Market.Domain.Aggregates.{Feature}.Errors;
using AE.Market.Domain.Common.Abstracts;
using AE.Market.Domain.Exceptions;
using MediatR;

namespace AE.Market.Application.Features.{Feature}.Commands.{Action};

internal sealed class {Action}CommandHandler(
    IRepository<{Entity}> repo
) : IRequestHandler<{Action}Command, Result<{Entity}Dto>>
{
    public async Task<Result<{Entity}Dto>> Handle({Action}Command request, CancellationToken ct)
    {
        var entity = await repo.FirstOrDefaultAsync(new ByXSpec(request.Param), ct);
        if (entity is null)
            return Result<{Entity}Dto>.Fail({Feature}Errors.{ErrorName});

        try
        {
            entity.DoSomething(...);      // domain method may throw DomainException
        }
        catch (DomainException ex)
        {
            return Result<{Entity}Dto>.Fail(new Error("Code", ex.Message));
        }

        var dto = new {Entity}Dto { ... };
        return Result<{Entity}Dto>.Success(dto);
    }
}
```
**Key rule:** Use `repo.GetBySpecWithTrackingAsync(spec, ct)` when entity needs to be tracked for saving. Use `repo.FirstOrDefaultAsync(spec, ct)` for read-only. Use `repo.AddAsync(entity, ct)` for new entities.

## Command Validator
```csharp
using FluentValidation;

namespace AE.Market.Application.Features.{Feature}.Commands.{Action};

public sealed class {Action}CommandValidator : AbstractValidator<{Action}Command>
{
    public {Action}CommandValidator()
    {
        RuleFor(x => x.Param1).NotEmpty();
        RuleFor(x => x.Param2).InclusiveBetween(1, 999);
    }
}
```

## Query (with caching)
```csharp
using AE.Market.Application.Common.Abstracts;
using AE.Market.Application.Features.{Feature}.DTOs;

namespace AE.Market.Application.Features.{Feature}.Queries.{Action};

public sealed record {Action}Query(
    Guid Param1
) : IBaseQuery<{Entity}Dto>, ICachedQuery
{
    public string CacheKey => CacheKeys.{Key}(Param1);
    TimeSpan? ICachedQuery.AbsoluteExpiration => TimeSpan.FromMinutes(5);
    TimeSpan? ICachedQuery.SlidingExpiration => null;
}
```

## Query Handler (with IReadRepository)
```csharp
using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Features.{Feature}.DTOs;
using AE.Market.Application.Features.{Feature}.Specs;
using AE.Market.Domain.Aggregates.{Feature};
using AE.Market.Domain.Aggregates.{Feature}.Errors;
using AE.Market.Domain.Common.Abstracts;
using MediatR;

namespace AE.Market.Application.Features.{Feature}.Queries.{Action};

internal sealed class {Action}QueryHandler(
    IReadRepository<{Entity}> repo
) : IRequestHandler<{Action}Query, Result<{Entity}Dto>>
{
    public async Task<Result<{Entity}Dto>> Handle({Action}Query request, CancellationToken ct)
    {
        var entity = await repo.FirstOrDefaultAsync(new ByXSpec(request.Param), ct);
        if (entity is null)
            return Result<{Entity}Dto>.Fail({Feature}Errors.{ErrorName});

        var dto = new {Entity}Dto { ... };
        return Result<{Entity}Dto>.Success(dto);
    }
}
```

## DTO
```csharp
namespace AE.Market.Application.Features.{Feature}.DTOs;

public sealed record {Entity}Dto
{
    public Guid Id { get; set; }
    // primitives only, enums as strings
}
```

## Specification
```csharp
using AE.Market.Domain.Aggregates.{Feature};
using AE.Market.Domain.Common.Specifications;

namespace AE.Market.Application.Features.{Feature}.Specs;

public sealed class ByXSpec : BaseSpecification<{Entity}>
{
    public ByXSpec(Guid x)
        : base(e => e.Property == x && !e.IsDeleted)
    { }
}
```
Always include `&& !e.IsDeleted` for soft-delete entities. Use `SetPagination()` and `SetOrderBy()` for paged/sorted specs. Use `AddInclude(q => q.NavProp)` to eager-load.

## CacheKeys
```csharp
namespace AE.Market.Application.Features.{Feature};

internal static class CacheKeys
{
    internal static string {Key}(Guid id) => $"{domain}-{subdomain}-{id}";
}
```

## Event Handler
```csharp
using AE.Market.Application.Common.Behaviors;
using AE.Market.Domain.Aggregates.{Feature}.Events;
using MediatR;

namespace AE.Market.Application.Features.{Feature}.Events;

internal sealed class {Action}Handler
    : INotificationHandler<DomainEventNotification<{Action}DomainEvent>>
{
    public Task Handle(
        DomainEventNotification<{Action}DomainEvent> notification,
        CancellationToken cancellationToken)
    {
        var evt = notification.DomainEvent;
        return Task.CompletedTask;
    }
}
```

## EF Configuration
```csharp
using AE.Market.Domain.Aggregates.{Feature};
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AE.Market.Infrastructure.Persistence.Configurations.{Schema};

internal sealed class {Entity}Configuration : IEntityTypeConfiguration<{Entity}>
{
    public void Configure(EntityTypeBuilder<{Entity}> builder)
    {
        builder.ToTable("table_name", "schema");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.X).IsRequired();
        builder.Property(x => x.Y).HasDefaultValue(0);
        builder.Property(x => x.Status).HasConversion<int>().HasDefaultValue({Status}.{Value});
        builder.HasIndex(x => x.X);
    }
}
```

## AppDbContext DbSet addition
Add import:
```csharp
using AE.Market.Domain.Aggregates.{Feature};
```
Add property in region:
```csharp
public DbSet<{Entity}> {Entities} { get; set; }
```

## Controller
```csharp
using AE.Market.API.Helpers;
using AE.Market.Application.Features.{Feature}.Commands.{Action};
using AE.Market.Application.Features.{Feature}.Queries.{Action};
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AE.Market.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public sealed class {Feature}Controller(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var result = await mediator.Send(new {Action}Query(...), ct);
        return result.ToActionResult();  // or .ToNotFoundActionResult(), .ToCreatedActionResult()
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] {Action}Command cmd, CancellationToken ct)
    {
        var result = await mediator.Send(cmd, ct);
        return result.ToCreatedActionResult();
    }
}
```

## Domain Test
```csharp
using AE.Market.Domain.Aggregates.{Feature};
using AE.Market.Domain.Aggregates.{Feature}.Events;
using AE.Market.Domain.Exceptions;
using FluentAssertions;

namespace AE.Market.Domain.Tests.Aggregates.{Feature};

public sealed class {Entity}Tests
{
    [Fact]
    public void Method_Scenario_ExpectedResult()
    {
        var entity = {Entity}.Create(Guid.NewGuid(), ...);
        entity.DoSomething();
        entity.Property.Should().Be(expected);
        entity.DomainEvents.Should().ContainSingle(e => e is {Action}DomainEvent);
    }

    [Fact]
    public void InvalidInput_ThrowsDomainException()
    {
        var act = () => {Entity}.Create(Guid.NewGuid(), invalidValue);
        act.Should().Throw<DomainException>()
            .Which.Code.Should().Be("Code");
    }
}
```

## Namespace Import Aliasing
When `Cart` in `AE.Market.Application.Features.Cart.Commands.X` conflicts with `Cart` entity in `AE.Market.Domain.Aggregates.Cart.Cart`:
```csharp
using CartAggregate = AE.Market.Domain.Aggregates.Cart.Cart;
```
Use `CartAggregate` everywhere in the file.

## Result Return Patterns
| Scenario | Return |
|---|---|
| Success with value | `Result<T>.Success(dto)` |
| Success no value | `Result.Success()` |
| Domain error | `Result.Fail(MyErrors.Something)` |
| Catch DomainException | `Result.Fail(new Error("Code", ex.Message))` |
| NotFound | `Result<T>.Fail(MyErrors.NotFound)` |

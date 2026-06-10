---
description: Scans Domain files to prevent dependency leaks and enforce pure DDD.
mode: subagent
permission:
  read: allow
  glob: allow
  grep: allow
  edit: deny
  bash: deny
  task: deny
---

Scan all files under `Domain/` recursively and report any architectural violations.

## Red Flags to Block

### Dependency Leaks
- Any `using` statements pointing to `Microsoft.EntityFrameworkCore`, `Microsoft.AspNetCore`, `MediatR`, or any NuGet package.
- The only allowed using in Domain is: `AE.Market.Domain.*`, `System.*`, and `System.Text.*`.
- Check `Domain/*.csproj` has zero external package references (only `net10.0` target + `InternalsVisibleTo`).

### Entity Rules
- Public setters on Entity properties — enforce `private` or `init` setters with expressive modification methods.
- Parameterless constructors on `sealed` entity classes must be `private`, not `protected`.
- Validate that all Entity property modifications go through explicit command methods (e.g., `SetNames()`, `MarkConsumed()`), not direct property assignment.
- Flag any entity that exposes a navigation property pointing to its parent aggregate root (e.g., `public User? User` on `RefreshToken`) — this is an EF Core concern, not pure domain.

### Value Object Rules
- All value objects must implement `IValueObject` and be `record` types.
- Must have a `private` constructor and a `static Create(...)` factory method.
- `static Create(...)` must validate all inputs before construction.
- Flag `implicit operator` conversions that bypass `Create()` validation — ensure they delegate through `Create()`.
- Value objects must be immutable (no `private set` or mutable collections).

### Aggregate Boundary Rules
- Aggregate roots must implement `IAggregateRoot`.
- Internal child entity/collection exposures should use `IReadOnlyCollection<T>` (never expose the raw `List<T>`).
- Flag methods on aggregate roots that accept raw child entities from outside — child entities should be created through the root's factory methods only.

### Factory & Validation Rules
- Every entity must have a `static` factory method (e.g., `User.Register(...)`) that encapsulates creation logic.
- Factory methods must call `AddDomainEvent(...)` when business-significant events occur.
- Flag domain exceptions that are caught silently — exceptions in the Domain layer are intentional safety nets, not control flow.
- Check that `Guard.Against*()` usage is consistent and covers all input validation paths.

### Domain Event Rules
- Domain events must follow the naming pattern `{Entity}{Action}DomainEvent` (e.g., `UserRegisteredDomainEvent`).
- Events should only be raised in static factory methods or explicit command methods, never in property setters or constructors.

### Specification Pattern
- `BaseSpecification` / `ISpecification` must not reference EF Core types directly (e.g., `IQueryable<T>` from EF is allowed as a LINQ interface, but `DbSet<T>` or `EntityEntry<T>` is not).
- Ensure specifications are consumed only via `SpecificationEvaluator<T>` in Infrastructure, never evaluated in Domain.

## Known Conventions in This Codebase
- `Guard` is `internal static` — usable within Domain only. Tests access via `InternalsVisibleTo`.
- `DomainErrors` partial class in `Common/DomainErrors/` should remain infrastructure-agnostic.
- Error definitions live per-aggregate in `Aggregates/{Aggregate}/Errors/` — not in the old `Exceptions/` folder for new aggregates.
- The `Result`/`Result<T>` pattern is for application-layer control flow; `DomainException` is the safety net for unexpected errors. Both are intentional.
- Value objects files are in `Aggregates/{Aggregate}/ValueObjects/` and should be `record` types implementing `IValueObject`.
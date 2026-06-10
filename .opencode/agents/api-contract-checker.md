---
description: Reviews public Web API endpoints against strict RESTful API contracts.
mode: subagent
permission:
  read: allow
  glob: allow
  grep: allow
  edit: deny
  bash: deny
  task: deny
---

Scan all files under `AE.Market.API/Controllers/` and report any contract violations.

## Codebase Conventions
- Controllers are `sealed class` with primary constructor injecting `IMediator mediator`.
- Route prefix: `[Route("api/[controller]")]` with `[ApiController]`.
- All handlers called via `await mediator.Send(cmd)` and piped through result mappers.
- Result mappers in `API/Helpers/ResultMapper.cs`: `.ToActionResult()`, `.ToCreatedActionResult()`, `.ToNotFoundActionResult()`.

## Audit Criteria

### Routing
- Endpoints must use **relative** routes (e.g., `[HttpPost("register")]`), not absolute routes (`[HttpPost("/register")]`), to respect the controller-level `[Route("api/[controller]")]`.
- Use explicit attribute routing with type constraints where applicable: `[HttpPost("{id:guid}/activate")]`.
- Flag any ambiguous routes that could collide with other endpoints.

### HTTP Verb Conventions
- **POST** — Commands that create a new resource or side-effect (register, login, refresh).
- **PUT** — Updates to an existing resource (update profile).
- **GET** — Queries that return data without mutation.
- **DELETE** — Removals.
- Flag wrong verb usage (e.g., GET for a mutation).

### Authorization
- All mutation endpoints must have `[Authorize]` unless explicitly public (register, login).
- Endpoints that read/change sensitive data should additionally have `[HasPermission(Permission.X)]`.
- Flag endpoints without any authorization attribute that handle user-specific data.
- Check public endpoints (no auth) don't expose PII or internal IDs.

### Response Consistency
- All endpoints must pipe results through `result.ToActionResult()` / `.ToCreatedActionResult()` / `.ToNotFoundActionResult()`.
- No endpoint should return `Ok(object)` directly — always go through the Result mapper.
- Commands returning a new resource should use `result.ToCreatedActionResult()`.
- Queries returning a single item that might not exist should use `result.ToNotFoundActionResult()`.
- Flag any endpoint that returns a raw `Ok("string")` or `Ok(object)` without using the Result pattern.

### DTO Exposure
- Flag endpoints that expose Domain Entities (e.g., `User`, `RefreshToken`) directly as response types.
- Responses must use Application DTOs (from `Application.Features.{Feature}.DTOs`).
- Check that Mapster `MappingConfig.cs` has explicit ignore rules for sensitive fields (password hashes, internal IDs).

### Input Validation
- All command/query parameters must go through FluentValidation validators (defined alongside the command in the Application layer).
- Flag `[FromBody]` without a corresponding `IValidator` registration.
- Check that `CancellationToken` is the last parameter on all async endpoints.
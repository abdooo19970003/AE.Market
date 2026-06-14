# API Contract Checker — RESTful Endpoint Audit

## Summary

| Severity | Count |
|----------|-------|
| Critical | 0 |
| Major | 3 |
| Minor | 4 |
| Info | 2 |

42 public endpoints reviewed across 6 controllers.

---

## Major

### M1. `AuthController.Register` returns 200 OK instead of 201 Created
`AuthController.cs:29` — `POST /api/auth/register` creates a new user resource. Must return 201 Created.

**Fix:** `result.ToActionResult()` → `result.ToCreatedActionResult()`.

### M2. `AuthController.RevokePermission` returns 200 OK instead of 204 No Content
`AuthController.cs:127` — `DELETE /api/auth/users/{userId:guid}/permissions`. DELETE must return 204.

**Fix:** `result.ToActionResult()` → `result.ToDeletedActionResult()`.

### M3. `AuthController.DeleteUser` returns 200 OK instead of 204 No Content
`AuthController.cs:158` — Same as M2.

**Fix:** `result.ToActionResult()` → `result.ToDeletedActionResult()`.

---

## Minor

### m1. `[HasPermission]` missing on 12 mutation endpoints across Catalog controllers
All `DELETE` endpoints have `[HasPermission(Permission.MutateProducts)]`, but `POST` (create) and `PUT` (update) only have `[Authorize]`.

| Controller | Endpoint |
|---|---|
| `ProductsController.cs:58,65,84` | POST `/api/products`, PUT `{id}`, POST `{id}/variants` |
| `CategoriesController.cs:33,41` | POST, PUT |
| `BrandsController.cs:33,41` | POST, PUT |
| `GroupUnitsController.cs:34,41,52` | POST, PUT, POST `{id}/units` |
| `ProductTaxCodesController.cs:33,41` | POST, PUT |

**Fix:** Add `[HasPermission(Permission.MutateProducts)]` to all 12 endpoints, or remove from DELETE — choose one consistent policy.

### m2. `AuthController.Register` — public endpoint leaks `UserId`
`TokensResponseDto.cs:5` — `Guid? UserId` exposed via unauthenticated endpoint.

**Fix:** Remove `UserId` from `TokensResponseDto` or include only on authenticated responses.

### m3. `GrantPermission`/`RevokePermission` uses `[FromQuery] Permission` (domain enum leak)
`AuthController.cs:119,132` — Domain type `Permission` leaked into API contract.

**Fix:** Create Application-level DTO or accept in request body.

### m4. `AuthController` missing `[FromBody]` on POST/PUT commands
`AuthController.cs:29,39,49` — Relies on implicit `[ApiController]` inference while Catalog controllers explicitly use `[FromBody]`.

**Fix:** Add `[FromBody]` to `RegisterCommand`, `LoginCommand`, `RefreshCommand`.

---

## Info

### i1. `GroupUnitsController` uses hardcoded `api/group-units` vs `api/[controller]`
Inconsistent with `ProductTaxCodesController` which uses default token → `api/ProductTaxCodes`.

### i2. `GrantPermission` uses `PUT` for non-idempotent action
Arguably should be `POST`. Minor style point.

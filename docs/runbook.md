# AE.Market Runbook

## Startup Sequence

### Development (Docker Compose)

```bash
# Start all services
docker compose up -d

# Or with override (adds Jaeger)
docker compose -f docker-compose.yml -f docker-compose.override.yml up -d
```

Services start in order: postgres → redis → elasticsearch → seq → api

### Production

```bash
# Start with production config
docker compose -f docker-compose.prod.yml --env-file .env up -d
```

Init container runs migrations before API starts.

## Health Checks

| Endpoint | Purpose | Expected Response |
|----------|---------|-------------------|
| `GET /health` | Liveness — is the app running? | 200 OK |
| `GET /health/ready` | Readiness — are dependencies available? | 200 OK or 503 |

### What /health/ready Checks

- PostgreSQL connection
- Redis connection
- Elasticsearch connection

## Common Failure Modes

### Database Connection Failed

**Symptom:** `/health/ready` returns 503, API logs "Connection refused"

**Recovery:**
1. Check PostgreSQL container: `docker compose ps postgres`
2. Check logs: `docker compose logs postgres`
3. Restart: `docker compose restart postgres`
4. If data corruption: `docker compose down -v && docker compose up -d`

### Redis Connection Failed

**Symptom:** Cache misses, FusionCache fallback to database

**Recovery:**
1. Check Redis container: `docker compose ps redis`
2. Check logs: `docker compose logs redis`
3. Restart: `docker compose restart redis`

### Elasticsearch Down

**Symptom:** Search queries fail, `/health/ready` returns 503

**Recovery:**
1. Check ES container: `docker compose ps elasticsearch`
2. Check logs: `docker compose logs elasticsearch`
3. Restart: `docker compose restart elasticsearch`
4. Reindex if needed: `curl -X POST http://localhost:9200/_reindex`

### JWT Secret Missing

**Symptom:** All authenticated requests return 401

**Recovery:**
1. Check environment variable: `Jwt__Secret`
2. Ensure it's set in `.env` or environment
3. Restart API container

## Database Migrations

### Apply Migrations

```bash
# Development (auto-applied on startup)
docker compose up -d

# Production (via init container)
docker compose -f docker-compose.prod.yml --env-file .env up -d

# Manual (if needed)
dotnet ef database update --project AE.Market.Infrastructure --startup-project AE.Market.API
```

### Rollback Migration

```bash
# Rollback to previous migration
dotnet ef database update <PreviousMigrationName> --project AE.Market.Infrastructure --startup-project AE.Market.API

# List migrations
dotnet ef migrations list --project AE.Market.Infrastructure --startup-project AE.Market.API
```

## Logging

### Seq UI

Access: `http://localhost:8082`
Default password: `password` (development only)

### Log Levels

Configure in `appsettings.json`:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  }
}
```

### Structured Logging Fields

- `CorrelationId` — request trace ID
- `UserId` — authenticated user ID
- `Elapsed` — request duration in ms

## Rollback Procedure

### Application Rollback

```bash
# Pull previous version
docker pull ghcr.io/{owner}/{repo}:{previous-tag}

# Update docker-compose.prod.yml image tag
# Restart
docker compose -f docker-compose.prod.yml --env-file .env up -d
```

### Database Rollback

1. Identify the migration to rollback to
2. Run: `dotnet ef database update {MigrationName}`
3. Verify application starts correctly
4. If issues persist, restore from backup

## Performance Monitoring

### Key Metrics

- Request duration (p50, p95, p99)
- Error rate (4xx, 5xx)
- Cache hit/miss ratio
- Database connection pool usage
- Queue depth (outbox messages)

### Jaeger Tracing

Access: `http://localhost:16686` (development)

View distributed traces for request flows across services.

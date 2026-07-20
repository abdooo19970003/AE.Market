# Load Testing

## Prerequisites

- [k6](https://k6.io/docs/get-started/installation/) installed
- API running locally or in staging

## Running Tests

```bash
# Product list (200 concurrent users)
k6 run scripts/product-list.js

# Product detail (200 concurrent users)
k6 run scripts/product-detail.js

# Search (100 concurrent users)
k6 run scripts/search.js

# Order placement (50 concurrent users)
k6 run scripts/order-placement.js
```

### Custom Base URL

```bash
k6 run --env BASE_URL=http://staging.example.com:8080 scripts/product-list.js
```

### Custom Test Credentials

```bash
k6 run --env TEST_EMAIL=user@example.com --env TEST_PASSWORD=Secret123 scripts/order-placement.js
```

## Thresholds

- **p95 < 500ms** — 95% of requests complete within 500ms
- **p99 < 1000ms** — 99% of requests complete within 1 second
- **Error rate < 1%** — Less than 1% of requests fail

## Interpreting Results

- `http_req_duration` — Request latency distribution
- `http_req_failed` — Failed request rate
- `http_reqs` — Total requests made
- `iterations` — Number of full test iterations

## Bottleneck Analysis

1. Check if p95 exceeds threshold
2. Identify slow endpoints from `http_req_duration` breakdown
3. Check server metrics (CPU, memory, database connections)
4. Review application logs for errors during test window
5. Profile slow handlers with distributed tracing (Jaeger)

import http from 'k6/http';
import { check, sleep } from 'k6';
import { SharedArray } from 'k6/data';

const BASE_URL = __ENV.BASE_URL || 'http://localhost:8080';

// In real usage, fetch product IDs from the API or use a predefined list
const productIds = new SharedArray('products', function () {
  return ['placeholder-id-1', 'placeholder-id-2', 'placeholder-id-3'];
});

export const options = {
  stages: [
    { duration: '30s', target: 200 },
    { duration: '2m', target: 200 },
    { duration: '30s', target: 0 },
  ],
  thresholds: {
    http_req_duration: ['p(95)<500', 'p(99)<1000'],
    http_req_failed: ['rate<0.01'],
  },
};

export default function () {
  const productId = productIds[Math.floor(Math.random() * productIds.length)];

  const res = http.get(`${BASE_URL}/api/products/${productId}`);

  check(res, {
    'status is 200': (r) => r.status === 200,
    'response time < 500ms': (r) => r.timings.duration < 500,
  });

  sleep(1);
}

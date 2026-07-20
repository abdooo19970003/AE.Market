import http from 'k6/http';
import { check, sleep } from 'k6';

const BASE_URL = __ENV.BASE_URL || 'http://localhost:8080';

const queries = ['laptop', 'phone', 'headphones', 'camera', 'tablet', 'watch', 'speaker'];

export const options = {
  stages: [
    { duration: '30s', target: 100 },
    { duration: '2m', target: 100 },
    { duration: '30s', target: 0 },
  ],
  thresholds: {
    http_req_duration: ['p(95)<500', 'p(99)<1000'],
    http_req_failed: ['rate<0.01'],
  },
};

export default function () {
  const query = queries[Math.floor(Math.random() * queries.length)];

  const res = http.get(`${BASE_URL}/api/search?q=${query}`);

  check(res, {
    'status is 200': (r) => r.status === 200,
    'response time < 500ms': (r) => r.timings.duration < 500,
  });

  sleep(1);
}

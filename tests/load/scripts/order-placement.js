import http from 'k6/http';
import { check, sleep } from 'k6';
import { randomIntBetween } from 'https://jslib.k6.io/k6-utils/1.4.0/index.js';

const BASE_URL = __ENV.BASE_URL || 'http://localhost:8080';
const EMAIL = __ENV.TEST_EMAIL || 'client@aemarket.com';
const PASSWORD = __ENV.TEST_PASSWORD || 'Client@12345';

export const options = {
  stages: [
    { duration: '30s', target: 50 },
    { duration: '2m', target: 50 },
    { duration: '30s', target: 0 },
  ],
  thresholds: {
    http_req_duration: ['p(95)<500', 'p(99)<1000'],
    http_req_failed: ['rate<0.01'],
  },
};

export function setup() {
  const loginRes = http.post(`${BASE_URL}/api/auth/login`, JSON.stringify({
    email: EMAIL,
    password: PASSWORD,
  }), {
    headers: { 'Content-Type': 'application/json' },
  });

  if (loginRes.status !== 200) {
    console.error(`Login failed: ${loginRes.status} ${loginRes.body}`);
    return { token: '' };
  }

  const body = JSON.parse(loginRes.body);
  const token = body.accessToken || '';
  return { token };
}

export default function (data) {
  if (!data.token) {
    sleep(1);
    return;
  }

  const headers = {
    'Content-Type': 'application/json',
    'Authorization': `Bearer ${data.token}`,
  };

  const productsRes = http.get(`${BASE_URL}/api/products?page=1&pageSize=5`, { headers });

  if (productsRes.status === 200) {
    const products = JSON.parse(productsRes.body);
    const items = products.items || products || [];
    if (items.length > 0) {
      const product = items[Math.floor(Math.random() * items.length)];
      const variantId = product.variants && product.variants.length > 0
        ? product.variants[0].id
        : null;

      if (variantId) {
        http.post(`${BASE_URL}/api/cart/items`, JSON.stringify({
          variantId: variantId,
          quantity: 1,
        }), { headers });

        const idempotencyKey = `load-${Date.now()}-${randomIntBetween(1, 99999)}`;
        const orderRes = http.post(`${BASE_URL}/api/orders`, null, {
          headers: {
            'Authorization': `Bearer ${data.token}`,
            'Idempotency-Key': idempotencyKey,
          },
        });

        check(orderRes, {
          'order status is 2xx': (r) => r.status >= 200 && r.status < 300,
          'response time < 500ms': (r) => r.timings.duration < 500,
        });
      }
    }
  }

  sleep(1);
}

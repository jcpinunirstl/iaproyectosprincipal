import http from 'k6/http';
import { check, group } from 'k6';

const BASE_URL = __ENV.BASE_URL || 'http://localhost:7142';
const TEST_USERNAME = __ENV.TEST_USERNAME || 'testuser';
const TEST_PASSWORD = __ENV.TEST_PASSWORD || 'TestPassword123!';

export const options = {
  stages: [
    { duration: '30s', target: 10 },
    { duration: '1m30s', target: 50 },
    { duration: '2m', target: 50 },
    { duration: '30s', target: 0 },
  ],
  thresholds: {
    http_req_duration: ['p(95)<800', 'p(99)<2000'],
    http_req_failed: ['rate<0.05'],
    'group_duration{group:::Login Load Test}': ['p(95)<1000'],
  },
};

export default function () {
  group('Login Load Test', () => {
    const loginPayload = JSON.stringify({
      username: TEST_USERNAME,
      password: TEST_PASSWORD,
    });

    const res = http.post(`${BASE_URL}/api/usuarios/login`, loginPayload, {
      headers: { 'Content-Type': 'application/json' },
      tags: { name: 'LoginLoadTest' },
    });

    check(res, {
      'Status 200': (r) => r.status === 200,
      'Respuesta con token': (r) => r.json('token') !== null,
      'Tiempo respuesta < 1s': (r) => r.timings.duration < 1000,
    });
  });
}

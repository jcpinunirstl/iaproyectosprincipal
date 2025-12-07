import http from 'k6/http';
import { check, group, fail } from 'k6';

const BASE_URL = __ENV.BASE_URL || 'http://localhost:5142';
const TEST_USERNAME = __ENV.TEST_USERNAME || 'jcarlos';
const TEST_PASSWORD = __ENV.TEST_PASSWORD || 'MiClaveSegura123';

export const options = {
  vus: 1,
  duration: '10s',
  thresholds: {
    http_req_duration: ['p(95)<500', 'p(99)<1000'],
    http_req_failed: ['rate<0.01'],
    'checks': ['rate>0.99'],
  },
};

export default function () {
  group('Smoke Test - User Registration', () => {
    const registerPayload = JSON.stringify({
      username: `${TEST_USERNAME}_${Date.now()}`,
      password: TEST_PASSWORD,
      email: `${TEST_USERNAME}_${Date.now()}@test.com`,
      nombre: 'Test User',
      telefono: '1234567890',
      genero: 'M',
    });

    const registerRes = http.post(`${BASE_URL}/api/usuarios/register`, registerPayload, {
      headers: { 'Content-Type': 'application/json' },
    });

    check(registerRes, {
      'Registro: status 200': (r) => r.status === 200,
      'Registro: tiene token': (r) => r.json('token') !== null,
      'Registro: username retornado': (r) => r.json('username') !== null,
    });
  });

  group('Smoke Test - User Login', () => {
    const loginPayload = JSON.stringify({
      username: TEST_USERNAME,
      password: TEST_PASSWORD,
    });

    const loginRes = http.post(`${BASE_URL}/api/usuarios/login`, loginPayload, {
      headers: { 'Content-Type': 'application/json' },
    });

    check(loginRes, {
      'Login: status 200': (r) => r.status === 200,
      'Login: retorna token': (r) => r.json('token') !== null && r.json('token').length > 0,
      'Login: retorna usuarioId': (r) => r.json('usuarioId') > 0,
      'Login: retorna username': (r) => r.json('username') !== null,
    });
  });
}

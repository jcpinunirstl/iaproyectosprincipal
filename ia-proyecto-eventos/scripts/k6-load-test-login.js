import http from 'k6/http';
import { check, group, fail } from 'k6';

const BASE_URL = __ENV.BASE_URL || 'http://localhost:5142';
const TEST_USERNAME = __ENV.TEST_USERNAME || 'jcarlos';
const TEST_PASSWORD = __ENV.TEST_PASSWORD || 'MiClaveSegura123';

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
  },
};

export default function () {
  group('Login Load Test', () => {

    const loginPayload = JSON.stringify({
      username: TEST_USERNAME,
      password: TEST_PASSWORD,
    });

    const headers = {
      'Content-Type': 'application/json',
    };

    const url = `${BASE_URL}/api/usuarios/login`;
    console.log(`URL: ${url}`);

    const res = http.post(url, loginPayload, { headers });

    // Si falla la conexión, evita errores y muestra info útil
    if (res.status === 0) {
      console.error('❌ Error de conexión. Verifica que tu API está corriendo.');
      console.error(`Detalles: ${res.error || 'Sin detalles'}`);
      return;
    }

    // Intentar parsear JSON con try/catch
    let jsonBody = null;
    try {
      jsonBody = res.json();
    } catch (e) {
      console.warn('⚠️ La respuesta NO es JSON válido');
    }

    check(res, {
      'Status 200': (r) => r.status === 200,
      'Respuesta contiene token': () => jsonBody && jsonBody.token,
      'Tiempo < 1s': (r) => r.timings.duration < 1000,
    });

    // Log si falla el login
    if (res.status !== 200) {
      console.warn(`⚠️ Login falló: status=${res.status}, body=${res.body}`);
    }
  });
}

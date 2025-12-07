import http from 'k6/http';
import { check, group, sleep } from 'k6';

const BASE_URL = __ENV.BASE_URL || 'http://localhost:5142';
const TEST_USERNAME = __ENV.TEST_USERNAME || 'testuser';
const TEST_PASSWORD = __ENV.TEST_PASSWORD || 'TestPassword123!';

export const options = {
  stages: [
    { duration: '30s', target: 50 },
    { duration: '1m', target: 100 },
    { duration: '2m', target: 100 },
    { duration: '1m', target: 200 },
    { duration: '1m', target: 200 },
    { duration: '30s', target: 0 },
  ],
  thresholds: {
    http_req_duration: ['p(95)<1500', 'p(99)<3000'],
    http_req_failed: ['rate<0.10'],
  },
};

function safeContentType(headers) {
  // k6 puede devolver headers con diferentes cases; devolvemos string seguro
  if (!headers) return '';
  return headers['Content-Type'] || headers['content-type'] || headers['content-type'.toLowerCase()] || '';
}

export default function () {
  group('Stress Test - Login', () => {
    const loginPayload = JSON.stringify({
      username: TEST_USERNAME,
      password: TEST_PASSWORD,
    });

    const res = http.post(`${BASE_URL}/api/usuarios/login`, loginPayload, {
      headers: { 'Content-Type': 'application/json' },
      tags: { name: 'LoginStressTest' },
      timeout: '60s', // opcional si tu API puede demorarse en alta carga
    });

    // Logs para depuración (quitar en ejecuciones masivas)
    if (__VU === 1 && __ITER === 0) { // solo imprimimos una vez para no spamear
      console.log('REQUEST URL:', `${BASE_URL}/api/usuarios/login`);
      console.log('STATUS:', res.status);
      console.log('CONTENT-TYPE HEADER:', JSON.stringify(res.headers));
      console.log('BODY (truncado a 1000 chars):', (res.body || '').toString().slice(0, 1000));
    }

    const contentType = safeContentType(res.headers);

    // Compruebas de forma segura (evita includes sobre undefined)
    check(res, {
      'Status 200/401 (aceptable en estrés)': (r) => r.status === 200 || r.status === 401,
      'Tiempo respuesta < 2s': (r) => r.timings && r.timings.duration < 2000,
      'Respuesta Content-Type contiene application/json (si presente)': (r) => {
        // Si header está presente, chequea JSON; si no está presente, devuelve false pero no Lanza error
        if (!contentType) return false;
        return contentType.includes('application/json');
      },
      'Respuesta parseable como JSON cuando status 200': (r) => {
        if (r.status !== 200) return true; // no necesitamos parsear otros códigos aquí
        try {
          const data = JSON.parse(r.body || '{}');
          return data !== null && typeof data === 'object';
        } catch (e) {
          // imprime para debug
          // console.log('JSON parse error:', e, 'body:', r.body);
          return false;
        }
      },
      'Token presente en respuesta (si 200)': (r) => {
        if (r.status !== 200) return true;
        try {
          const data = JSON.parse(r.body || '{}');
          return !!(data && (data.token || data.access_token)); // admite ambos nombres comunes
        } catch (e) {
          return false;
        }
      },
    });

    // Si la respuesta no es 200, y no es 401, imprime el cuerpo (útil para debug)
    if (res.status !== 200 && res.status !== 401) {
      console.log(`VU ${__VU} ITER ${__ITER} -> Unexpected status ${res.status}`);
      console.log('Response headers:', JSON.stringify(res.headers));
      console.log('Response body (first 2000 chars):', (res.body || '').toString().slice(0, 2000));
    }

    sleep(1);
  });
}

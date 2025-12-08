# K6 Load Testing Scripts

Pruebas de carga y rendimiento usando **k6** (Grafana k6).

## Archivos

- **k6-smoke-test-login.js** - Validación básica del sistema (1 usuario, 10 segundos)
- **k6-load-test-login.js** - Prueba de carga gradual (0→50 usuarios, ~4.5 minutos)
- **k6-stress-test-login.js** - Prueba de estrés (0→200 usuarios, ~5.5 minutos)
- **run-k6-tests.sh** - Script ejecutor para Linux/Mac
- **run-k6-tests.bat** - Script ejecutor para Windows

## Requisitos

- **k6** instalado: https://k6.io/docs/getting-started/installation
- **API .NET** ejecutándose en `http://localhost:5142`

## Ejecución Rápida

### Linux/Mac
```bash
# Desde esta carpeta
./run-k6-tests.sh smoke    # Smoke test
./run-k6-tests.sh load     # Load test
./run-k6-tests.sh stress   # Stress test
./run-k6-tests.sh all      # Todas las pruebas
```

### Windows
```cmd
# Desde esta carpeta
run-k6-tests.bat smoke     REM Smoke test
run-k6-tests.bat load      REM Load test
run-k6-tests.bat stress    REM Stress test
run-k6-tests.bat all       REM Todas las pruebas
```

### Comando Directo (Cualquier SO)
```bash
# Smoke test
k6 run --vus 1 --duration 10s k6-smoke-test-login.js

# Load test (toma ~4.5 minutos)
k6 run k6-load-test-login.js

# Stress test (toma ~5.5 minutos)
k6 run k6-stress-test-login.js
```

## Variables de Entorno

```bash
# URLs personalizadas
BASE_URL=http://localhost:7142 k6 run k6-smoke-test-login.js

# Credenciales personalizadas
TEST_USERNAME=myuser \
TEST_PASSWORD=mypass \
k6 run k6-smoke-test-login.js

# Combinadas
BASE_URL=http://api.example.com \
TEST_USERNAME=ciuser \
TEST_PASSWORD=CIPassword123! \
k6 run k6-load-test-login.js
```

## Reportes

Los reportes se generan automáticamente en `k6-reports/`:

```
k6-reports/
├── smoke-results.json      # Métricas detalladas (smoke test)
├── smoke-report.html       # Reporte visual
├── load-results.json       # Métricas detalladas (load test)
├── load-report.html        # Reporte visual
├── stress-results.json     # Métricas detalladas (stress test)
└── stress-report.html      # Reporte visual
```

## Umbrales de Éxito

### Smoke Test (10 segundos, 1 usuario)
- ✅ p(95) latencia < 500ms
- ✅ p(99) latencia < 1000ms
- ✅ Error rate < 1%

### Load Test (4.5 minutos, hasta 50 usuarios)
- ✅ p(95) latencia < 800ms
- ✅ p(99) latencia < 2000ms
- ✅ Error rate < 5%

### Stress Test (5.5 minutos, hasta 200 usuarios)
- ✅ p(95) latencia < 1500ms
- ✅ p(99) latencia < 3000ms
- ✅ Error rate < 10%

## Troubleshooting

### "API no está disponible"
```bash
# Asegúrate de que la API .NET está ejecutándose
curl http://localhost:5142/api/usuarios

# Si no responde, inicia la API:
cd ../ia-proyecto-eventos
dotnet run
```

### "k6: command not found"
```bash
# Instala k6:
# macOS
brew install k6

# Ubuntu/Debian
sudo apt-get install k6

# Windows
choco install k6
# O descarga desde: https://github.com/grafana/k6/releases
```

### "Cannot find module 'k6/http'"
- Este error ocurre solo fuera de k6
- Los scripts están diseñados para ejecutarse CON k6, no con Node.js
- Usa: `k6 run script.js` no `node script.js`

## Integración CI/CD

Estos scripts se ejecutan automáticamente en GitHub Actions:
- `.github/workflows/k6-load-testing.yml`

Documentación completa: `../../TESTING.md`

## Documentación Completa

Ver `../../TESTING.md` para:
- Configuración detallada
- Interpretación de resultados
- Mejores prácticas
- Resolución de problemas avanzada

# GitHub Actions Workflow - Guía de Referencia

## Resumen de Cambios v2.0

El workflow de GitHub Actions ha sido **completamente replanteado** para resolver problemas de confiabilidad y escalabilidad.

## Problemas Resueltos ✅

### ❌ Job `setup-test-user` → ✅ Eliminado
- **Problema**: Intentaba crear usuario dinámicamente pero la API podría no estar lista
- **Problema**: Las credenciales no se transmitían correctamente a otros jobs
- **Solución**: Usar credenciales fijas como variables de entorno globales

### ❌ Dependencias entre jobs → ✅ Independientes
- **Problema**: Si un job fallaba, todos los dependientes fallaban
- **Problema**: Difícil escalar y paralelizar pruebas
- **Solución**: Cada job es independiente y compila su propia API

### ❌ Condiciones complejas → ✅ Simplificadas
- **Problema**: Lógica de if complicada y inconsistente
- **Solución**: Condiciones claras basadas en eventos

### ❌ Falta de tolerancia a errores → ✅ Mejorada
- **Problema**: Un fallo en k6 detenía la generación de reportes
- **Solución**: `continue-on-error: true` + `if: always()`

## Arquitectura Nueva

```
┌─────────────────┐
│  test-setup     │  Compila API (.NET 9.0)
│  (2 min)        │  Cache de dependencias
└────────┬────────┘
         │
    ┌────┴─────┬──────────┬──────────┐
    │           │          │          │
    ▼           ▼          ▼          ▼
┌────────┐ ┌────────┐ ┌────────┐ ┌────────┐
│ smoke  │ │ load   │ │stress  │ │ manual │
│ auto   │ │ manual │ │ manual │ │ only   │
│(1.5min)│ │(7min)  │ │(10min) │ │        │
└────────┘ └────────┘ └────────┘ └────────┘
    │           │          │          │
    └───────────┴──────────┴──────────┘
           ↓
      Reports HTML
      (30 days retention)
```

## Variables de Entorno Globales

```yaml
env:
  TEST_USERNAME: 'ciuser'
  TEST_PASSWORD: 'CIPassword123!'
```

**Nota**: Son credenciales de CI/CD, no de producción.

## Jobs Disponibles

### 1. `test-setup` (Requisito)
```yaml
Compilación Central
├── .NET 9.0.x
├── Restore dependencies
├── Build Release
└── Cache NuGet
```

**Ejecución**: Siempre (es requisito para otros jobs)

### 2. `smoke-test`
```yaml
Validación Básica
├── API en background
├── Wait ready (60s)
├── k6: 1 usuario, 10s
├── Genera HTML report
├── Valida >= 95% availability
└── Sube artefactos
```

**Triggers**:
- Push a `main` o `develop`
- Pull Request a `main` o `develop`
- Manual: `test_type: 'smoke'` o `'all'`

**Duración**: ~1.5 minutos
**Tolerancia**: p(95) < 500ms, error rate < 1%

### 3. `load-test`
```yaml
Prueba de Carga Gradual
├── API en background
├── Wait ready (60s)
├── k6: ramp-up 0→50 usuarios (~4-5 min)
├── Genera HTML report
├── Valida >= 95% availability
└── Sube artefactos (timeout 10 min)
```

**Triggers**: Manual only
- `test_type: 'load'` o `'all'`

**Duración**: ~7 minutos
**Tolerancia**: p(95) < 800ms, error rate < 5%

### 4. `stress-test`
```yaml
Prueba de Punto de Ruptura
├── API en background
├── Wait ready (60s)
├── k6: ramp-up 0→200 usuarios (~5-6 min)
├── Genera HTML report
├── Acepta cualquier disponibilidad
└── Sube artefactos (timeout 15 min)
```

**Triggers**: Manual only
- `test_type: 'stress'` o `'all'`

**Duración**: ~10 minutos
**Tolerancia**: p(95) < 1500ms, error rate < 10%

## Cómo Ejecutar

### Automáticamente (Push/PR)
```bash
git push origin main  # Ejecuta smoke-test automáticamente
git push origin develop  # Ejecuta smoke-test automáticamente
```

### Manualmente desde GitHub UI

1. **Ir a**: `Actions` → `K6 Load Testing - Login`
2. **Click**: `Run workflow`
3. **Seleccionar**:
   - `smoke`: Solo humo (rápido)
   - `load`: Solo carga (~7 min)
   - `stress`: Solo estrés (~10 min)
   - `all`: Todas secuencialmente (~20 min)
4. **Click**: `Run workflow`

### Verificar Resultados

1. **Live**: Click en el job en ejecución
2. **Logs**: Detalle en cada paso
3. **Artifacts**: Descargar `k6-*-test-reports/`
   - `smoke-results.json`
   - `smoke-report.html` (abrir en navegador)

## Métricas y Thresholds

### Smoke Test
```
✓ p(95) latencia < 500ms
✓ p(99) latencia < 1000ms  
✓ Error rate < 1%
✓ Disponibilidad >= 95%
```

### Load Test
```
✓ p(95) latencia < 800ms
✓ p(99) latencia < 2000ms
✓ Error rate < 5%
✓ Disponibilidad >= 95%
```

### Stress Test
```
✓ p(95) latencia < 1500ms
✓ p(99) latencia < 3000ms
✓ Error rate < 10%
✓ Disponibilidad >= 85% (informativo)
```

## Troubleshooting del Workflow

### ❌ API no responde
**Log**: `Waiting for API to start... timeout`

**Solución**:
1. Revisar build logs de .NET
2. Verificar que no hay errores de compilación
3. Aumentar timeout si es red lenta

### ❌ k6 falla pero no ves reportes
**Esperado**: Los reportes se generan igual con `continue-on-error: true`

**Verificar**: Artifacts → `k6-*-test-reports/`

### ❌ Job depende de setup fallido
**No debería pasar**: Cada job es independiente ahora

**Si pasa**:
1. Click en `test-setup` job
2. Revisar logs de build
3. Asegurar .NET 9.0.x disponible

### ❌ Credenciales no funcionan
**Usuario**: `ciuser`
**Password**: `CIPassword123!`

**Si falla el login**:
1. Verificar que el usuario se registró
2. Revisar logs de API
3. Validar que POST `/api/usuarios/register` funciona

## Archivos Clave

- `.github/workflows/k6-load-testing.yml` - Workflow definition
- `IaProyectoEventos.Tests/scripts/k6-smoke-test-login.js` - Smoke test
- `IaProyectoEventos.Tests/scripts/k6-load-test-login.js` - Load test
- `IaProyectoEventos.Tests/scripts/k6-stress-test-login.js` - Stress test
- `IaProyectoEventos.Tests/scripts/run-k6-tests.sh` - Script ejecutor Linux/Mac
- `IaProyectoEventos.Tests/scripts/run-k6-tests.bat` - Script ejecutor Windows
- `TESTING.md` - Documentación completa de tests

## Cambios Recientes

### v2.0 (Current)
- ✅ Arquitectura replanteada
- ✅ Jobs independientes
- ✅ Credenciales fijas
- ✅ Mejor manejo de errores
- ✅ Timeouts inteligentes
- ✅ .NET 9.0.x

### v1.0 (Anterior)
- ❌ Job `setup-test-user` problemático
- ❌ Dependencias cascada
- ❌ Condiciones complejas
- ❌ .NET 8.0.x

## Próximas Mejoras (Sugerencias)

- [ ] Integrar con Grafana Cloud para k6
- [ ] Webhook de notificaciones a Slack
- [ ] Cache de resultados para comparar tendencias
- [ ] Database tests integrados
- [ ] API security scanning
- [ ] Performance regression detection

## Recursos

- [k6 Documentation](https://k6.io/docs/)
- [GitHub Actions Documentation](https://docs.github.com/en/actions)
- [.NET Testing Documentation](https://learn.microsoft.com/en-us/dotnet/core/testing/)

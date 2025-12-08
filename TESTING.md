# Documentación de Pruebas - Ia Proyectos Principal

## Índice
1. [Pruebas Unitarias (.NET/C#)](#pruebas-unitarias-netc)
2. [Pruebas de Carga y Rendimiento (k6)](#pruebas-de-carga-y-rendimiento-k6)
3. [Ejecución de Pruebas](#ejecución-de-pruebas)
4. [Integración con CI/CD](#integración-con-cicd)
5. [Reportes de Pruebas](#reportes-de-pruebas)

---

## Pruebas Unitarias (.NET/C#)

### Información General

- **Framework**: xUnit
- **Ubicación**: `IaProyectoEventos.Tests/`
- **Target Framework**: .NET 9.0
- **Cobertura**: Incluye pruebas de controladores, seguridad y validación
- **Dependencias**:
  - `xunit` (v2.9.2)
  - `Microsoft.NET.Test.Sdk` (v17.12.0)
  - `xunit.runner.visualstudio` (v2.8.2)
  - `coverlet.collector` (v6.0.2)

### Estructura del Proyecto de Tests

```
IaProyectoEventos.Tests/
├── EventosControllerTests.cs           # Pruebas del controlador de Eventos
├── UsuariosControllerTests.cs          # Pruebas del controlador de Usuarios
├── PersonasControllerTests.cs          # Pruebas del controlador de Personas
├── RegistroAsistenciasControllerTests.cs # Pruebas del registro de asistencias
├── TipoEventosControllerTests.cs       # Pruebas del controlador de tipos de evento
├── SecurityTests.cs                    # Pruebas de seguridad y JWT
└── IaProyectoEventos.Tests.csproj      # Configuración del proyecto de tests
```

### Archivos de Test Detallados

#### 1. **EventosControllerTests.cs**
Pruebas funcionales del controlador de Eventos

**Casos de Prueba:**
- `GetEventos_ReturnsAll()`: Verifica que se devuelven todos los eventos
- `GetEvento_ReturnsNotFound_WhenMissing()`: Verifica que retorna 404 cuando falta un evento
- `PostEvento_CreatesAndReturnsCreated()`: Verifica creación correcta de eventos
- `PostEvento_ReturnsBadRequest_WhenTipoMissing()`: Valida que el tipo de evento sea obligatorio
- `PostEvento_ReturnsBadRequest_WhenUsuarioMissing()`: Valida que el usuario sea requerido

**Estrategia de Testing:**
- Usa base de datos en memoria con xUnit
- Crea contextos aislados con nombres únicos para evitar conflictos
- Valida respuestas HTTP y tipos de retorno

---

#### 2. **UsuariosControllerTests.cs**
Pruebas del controlador de Usuarios, autenticación y registro

**Casos de Prueba:**
- `GetUsuarios_ReturnsAll()`: Obtiene todos los usuarios
- `GetUsuario_ReturnsNotFound_WhenMissing()`: Retorna 404 cuando falta usuario
- `PostUsuario_CreatesAndReturnsCreated()`: Crea usuario correctamente
- `RegisterAndLogin_Workflow()`: Flujo completo de registro y login
  - Registra nuevo usuario
  - Obtiene token JWT
  - Realiza login con las mismas credenciales
  - Verifica que ambas operaciones devuelven el mismo usuarioId
- `Register_Duplicate_ReturnsConflict()`: Valida que no se registren usuarios duplicados

**Configuración de Tests:**
```csharp
// Configuración JWT para tests
{
    "Jwt:Key": "super_secret_test_key_which_is_long_enough_1234567890",
    "Jwt:Issuer": "test",
    "Jwt:Audience": "test"
}
```

---

#### 3. **SecurityTests.cs**
Pruebas exhaustivas de seguridad, JWT y validación

**Casos de Prueba - JWT:**
- `Jwt_HeaderAndPayloadContainExpectedValues()`: Valida estructura JWT
  - Verifica algoritmo: HS256
  - Verifica tipo: JWT
  - Valida claims: sub (usuario ID) y unique_name (username)
- `Headers_JwtHeaderContainsAlgAndTyp()`: Verifica headers JWT correctos

**Casos de Prueba - Autorización:**
- `Authorization_GetEventosByUsuario_RequiresUserClaim()`: 
  - Sin claims: retorna 401 Unauthorized
  - Con claims válidos: retorna eventos del usuario

**Casos de Prueba - Inyección SQL:**
- `Injection_RegisterUsernameWithSqlLikeContent_DoesNotBreak()`:
  - Username: `evil'; DROP TABLE Usuarios; --`
  - Valida que el usuario se guarda literalmente
  - Verifica que la tabla no se ve afectada
- `SqlInjection_LoginBypassAttempt_ReturnsUnauthorized()`:
  - Intenta bypass con: `' OR '1'='1`
  - Valida que retorna 401

**Casos de Prueba - Validación:**
- `Validation_PostUsuarioMissingPasswordHash_ReturnsBadRequest()`:
  - Valida que passwordHash es obligatorio
  - Retorna 400 Bad Request

---

#### 4. **PersonasControllerTests.cs**
Pruebas del controlador de Personas

**Casos de Prueba:**
- `GetPersonas_ReturnsAll()`: Obtiene todas las personas
- `GetPersona_ReturnsNotFound_WhenMissing()`: Retorna 404 cuando falta
- `PostPersona_CreatesAndReturnsCreated()`: Crea persona correctamente
- `DeletePersona_ReturnsBadRequest_WhenHasAsistencias()`:
  - No permite eliminar si tiene registros de asistencia
  - Retorna 400 Bad Request
- `DeletePersona_ReturnsNotFound_WhenMissing()`: Retorna 404 si no existe

**TipoEventosControllerErrorTests:**
- `GetTipoEvento_ReturnsNotFound_WhenMissing()`: Valida tipos de evento

---

#### 5. **RegistroAsistenciasControllerTests.cs**
Pruebas del registro de asistencias a eventos

**Casos de Prueba:**
- `GetRegistroAsistencias_ReturnsAll()`: Obtiene todos los registros
- `PostRegistroAsistencia_CreatesAndReturnsCreated()`:
  - Crea registro con evento y persona válidos
  - Valida referencias a Evento y Persona
- `PostRegistroAsistencia_ReturnsBadRequest_WhenMissingReferences()`:
  - Valida que evento y persona sean obligatorios
  - Retorna 400 si faltan referencias
- `PutRegistroAsistencia_ReturnsBadRequest_WhenEventoOrPersonaMissingOrIdMismatch()`:
  - ID mismatch: retorna 400
  - Evento faltante: retorna 400
  - Persona faltante: retorna 400

---

#### 6. **TipoEventosControllerTests.cs**
Pruebas del controlador de tipos de eventos

**Casos de Prueba:**
- `GetTipoEventos_ReturnsAll()`: Obtiene todos los tipos
- `GetTipoEvento_ReturnsNotFound_WhenMissing()`: Retorna 404 cuando falta
- `PostTipoEvento_CreatesAndReturnsCreated()`: Crea tipo correctamente

---

### Ejecutar Pruebas Unitarias

#### Desde Visual Studio
```bash
# Abrir el Test Explorer
# View > Test Explorer
# Click en "Run All Tests"
```

#### Desde Línea de Comandos
```bash
# Ejecutar todas las pruebas
dotnet test IaProyectoEventos.Tests/IaProyectoEventos.Tests.csproj

# Ejecutar con salida detallada
dotnet test IaProyectoEventos.Tests/IaProyectoEventos.Tests.csproj --verbosity normal

# Ejecutar con resultados en formato trx
dotnet test IaProyectoEventos.Tests/IaProyectoEventos.Tests.csproj --logger "trx"

# Ejecutar con cobertura de código
dotnet test IaProyectoEventos.Tests/IaProyectoEventos.Tests.csproj /p:CollectCoverage=true
```

---

## Pruebas de Carga y Rendimiento (k6)

### Información General

- **Herramienta**: k6 (Grafana k6)
- **Ubicación**: `ia-proyecto-eventos/scripts/`
- **Lenguaje**: JavaScript
- **API Testeada**: Endpoint de login `/api/usuarios/login`
- **Documentación**: https://k6.io/

### Instalación de k6

#### Windows
```powershell
# Con Chocolatey
choco install k6

# Con MSI descargado
# https://github.com/grafana/k6/releases
```

#### Linux/Mac
```bash
# Con apt (Debian/Ubuntu)
sudo apt-get update
sudo apt-get install k6

# Con Homebrew (macOS)
brew install k6

# Con Snap
sudo snap install k6
```

#### Verificar instalación
```bash
k6 version
```

### Estructura de Pruebas k6

```
ia-proyecto-eventos/scripts/
├── k6-smoke-test-login.js           # Pruebas de humo (smoke test)
├── k6-load-test-login.js            # Pruebas de carga (load test)
├── k6-stress-test-login.js          # Pruebas de estrés (stress test)
├── run-k6-tests.sh                  # Script ejecutor (Linux/Mac)
├── run-k6-tests.bat                 # Script ejecutor (Windows)
├── test.js                          # Utilidades de prueba
└── validate-k6-results.js           # Validación de resultados
```

---

### Tipos de Pruebas k6

#### 1. **Smoke Test** (`k6-smoke-test-login.js`)

**Propósito**: Verificación básica de que el sistema funciona

**Configuración:**
```javascript
export const options = {
  vus: 1,                           // 1 usuario virtual
  duration: '10s',                  // 10 segundos
  thresholds: {
    http_req_duration: ['p(95)<500', 'p(99)<1000'],  // Latencia
    http_req_failed: ['rate<0.01'],                  // Tasa de fallo < 1%
    'checks': ['rate>0.99'],                         // Validaciones > 99%
  },
};
```

**Casos Probados:**
1. **Registro de Usuario**
   - Endpoint: `POST /api/usuarios/register`
   - Validaciones:
     - Status: 200
     - Respuesta contiene token
     - Respuesta contiene username

2. **Login de Usuario**
   - Endpoint: `POST /api/usuarios/login`
   - Validaciones:
     - Status: 200
     - Token presente en respuesta
     - usuarioId > 0
     - Username en respuesta

**Duración Total**: ~10 segundos

---

#### 2. **Load Test** (`k6-load-test-login.js`)

**Propósito**: Verificar rendimiento bajo carga gradual

**Configuración:**
```javascript
export const options = {
  stages: [
    { duration: '30s', target: 10 },    // Ramp-up a 10 usuarios
    { duration: '1m30s', target: 50 },  // Aumento a 50 usuarios
    { duration: '2m', target: 50 },     // Mantener 50 usuarios
    { duration: '30s', target: 0 },     // Ramp-down a 0
  ],
  thresholds: {
    http_req_duration: ['p(95)<800', 'p(99)<2000'],  // Percentiles de latencia
    http_req_failed: ['rate<0.05'],                  // Tasa de fallo < 5%
  },
};
```

**Fases:**
1. **Ramp-up** (30s): 0 → 10 usuarios
2. **Ramp-up** (90s): 10 → 50 usuarios
3. **Plateau** (120s): 50 usuarios constantes
4. **Ramp-down** (30s): 50 → 0 usuarios

**Métricas Evaluadas:**
- p(95) < 800ms: 95% de solicitudes < 800ms
- p(99) < 2000ms: 99% de solicitudes < 2000ms
- Tasa de error < 5%

**Duración Total**: ~4.5 minutos

---

#### 3. **Stress Test** (`k6-stress-test-login.js`)

**Propósito**: Encontrar límites del sistema bajo carga extrema

**Configuración:**
```javascript
export const options = {
  stages: [
    { duration: '30s', target: 50 },    // 0 → 50 usuarios
    { duration: '1m', target: 100 },    // 50 → 100 usuarios
    { duration: '2m', target: 100 },    // 100 constantes
    { duration: '1m', target: 200 },    // 100 → 200 usuarios
    { duration: '1m', target: 200 },    // 200 constantes
    { duration: '30s', target: 0 },     // 200 → 0 usuarios
  ],
  thresholds: {
    http_req_duration: ['p(95)<1500', 'p(99)<3000'],
    http_req_failed: ['rate<0.10'],     // Tolerancia mayor (10%)
  },
};
```

**Fases:**
1. Ramp-up a 50 usuarios (30s)
2. Ramp-up a 100 usuarios (60s)
3. Plateau con 100 usuarios (120s)
4. Ramp-up a 200 usuarios (60s)
5. Plateau con 200 usuarios (60s)
6. Ramp-down (30s)

**Picos Testados**: Hasta 200 usuarios virtuales simultáneos

**Duración Total**: ~5.5 minutos

**Nota**: En stress tests se acepta mayor tasa de error para encontrar límites

---

### Configuración Común

Todas las pruebas usan variables de entorno:

```javascript
const BASE_URL = __ENV.BASE_URL || 'http://localhost:5142';
const TEST_USERNAME = __ENV.TEST_USERNAME || 'jcarlos';
const TEST_PASSWORD = __ENV.TEST_PASSWORD || 'MiClaveSegura123';
```

---

## Ejecución de Pruebas

### Requisitos Previos

1. **API .NET ejecutándose**:
   ```bash
   cd ia-proyecto-eventos
   dotnet run
   ```
   La API estará disponible en: `http://localhost:5142`

2. **k6 instalado** (para pruebas de carga):
   ```bash
   k6 version
   ```

3. **PowerShell/bash disponible**

---

### Ejecutar Pruebas k6 en Linux/Mac

#### Opción 1: Script automatizado
```bash
cd ia-proyecto-eventos/scripts

# Smoke test
./run-k6-tests.sh smoke

# Load test
./run-k6-tests.sh load

# Stress test
./run-k6-tests.sh stress

# Todas las pruebas
./run-k6-tests.sh all
```

#### Opción 2: Comando directo
```bash
cd ia-proyecto-eventos/scripts

# Smoke test
k6 run --vus 1 --duration 10s k6-smoke-test-login.js

# Load test
k6 run k6-load-test-login.js

# Stress test
k6 run k6-stress-test-login.js
```

#### Opción 3: Con variables de entorno personalizadas
```bash
BASE_URL=http://localhost:7142 \
TEST_USERNAME=miusuario \
TEST_PASSWORD=MiPassword123 \
k6 run k6-load-test-login.js
```

---

### Ejecutar Pruebas k6 en Windows

#### Opción 1: Script batch automatizado
```cmd
cd ia-proyecto-eventos\scripts

REM Smoke test
run-k6-tests.bat smoke

REM Load test
run-k6-tests.bat load

REM Stress test
run-k6-tests.bat stress

REM Todas las pruebas
run-k6-tests.bat all
```

#### Opción 2: Comando directo
```cmd
cd ia-proyecto-eventos\scripts

k6 run --vus 1 --duration 10s k6-smoke-test-login.js

k6 run k6-load-test-login.js

k6 run k6-stress-test-login.js
```

#### Opción 3: Con variables personalizadas (PowerShell)
```powershell
$env:BASE_URL = "http://localhost:7142"
$env:TEST_USERNAME = "miusuario"
$env:TEST_PASSWORD = "MiPassword123"
k6 run k6-load-test-login.js
```

---

### Resultados de Ejecución

Al ejecutar pruebas k6, se generan:

1. **Resumen en consola**:
   ```
   ✓ Status 200
   ✓ Respuesta contiene token
   ✓ Tiempo < 1s
   ```

2. **Archivo JSON de resultados**: `k6-reports/{test-type}-results.json`

3. **Reporte HTML**: `k6-reports/{test-type}-report.html`

---

## Integración con CI/CD

### GitHub Actions Workflow

**Archivo**: `.github/workflows/k6-load-testing.yml`

**Triggers:**
- Push a ramas: `main`, `develop`
- Pull requests a: `main`, `develop`
- Ejecución manual (workflow_dispatch)

**Flujo de Ejecución:**

#### 1. Job: `setup-test-user`
- Compila la API .NET 8.0
- Inicia la API en background
- Crea usuario de prueba dinámico
- Comparte credenciales con otros jobs

#### 2. Job: `smoke-test` (Siempre ejecuta)
- Ejecuta smoke tests (10s)
- Genera reporte HTML
- Valida disponibilidad >= 99%
- Sube artefactos

#### 3. Job: `load-test` (Si se selecciona)
- Ejecuta load tests (~4.5 min)
- Genera reporte HTML
- Valida disponibilidad >= 99%
- Sube artefactos

#### 4. Job: `stress-test` (Si se selecciona)
- Ejecuta stress tests (~5.5 min)
- Genera reporte HTML
- Acepta disponibilidad < 99% (esperado)
- Sube artefactos

**Opciones de ejecución manual:**
- `smoke`: Solo smoke tests
- `load`: Solo load tests
- `stress`: Solo stress tests
- `all`: Todas las pruebas

**Artefactos Generados:**
- `k6-smoke-test-reports/`: Reportes de smoke test
- `k6-load-test-reports/`: Reportes de load test
- `k6-stress-test-reports/`: Reportes de stress test

**Retención**: 30 días

---

## Reportes de Pruebas

### Formato de Reportes

#### Reporte JSON (`{test-type}-results.json`)
```json
{
  "metrics": {
    "http_reqs": {
      "values": {
        "count": 150,
        "fails": 2
      }
    },
    "http_req_duration": {
      "values": {
        "p(95)": 750,
        "p(99)": 1500
      }
    }
  }
}
```

#### Reporte HTML (`{test-type}-report.html`)
```
📊 K6 {Test Type} Test Report
├── Disponibilidad: X%
├── Total de solicitudes: XXX
├── Solicitudes exitosas: XXX
├── Solicitudes fallidas: X
└── Disponibilidad: X%
```

**Criterios de Éxito:**
- ✅ Disponibilidad >= 99%
- ✅ p(95) latencia dentro de límite
- ✅ p(99) latencia dentro de límite
- ✅ Tasa de error < umbral

---

### Análisis de Resultados

#### Lectura de Métricas

**Percentiles de Latencia:**
- `p(95)`: El 95% de solicitudes completadas en X ms
- `p(99)`: El 99% de solicitudes completadas en X ms

**Ejemplo:**
```
p(95)<800  → 95% de solicitudes < 800ms ✓
p(99)<2000 → 99% de solicitudes < 2000ms ✓
```

**Tasa de Error:**
```
rate<0.05  → Menos del 5% de solicitudes fallidas ✓
http_req_failed: 10/200 = 5% → Marginal
http_req_failed: 20/200 = 10% → Falla
```

---

### Interpretación de Pruebas

#### Smoke Test
- **Objetivo**: Validar funcionalidad básica
- **Éxito**: Todas las validaciones pasan sin errores
- **Indica**: El sistema está operativo

#### Load Test
- **Objetivo**: Validar rendimiento bajo carga típica
- **Éxito**: Mantiene latencia aceptable con 50 usuarios
- **Indica**: El sistema puede manejar carga esperada

#### Stress Test
- **Objetivo**: Encontrar punto de ruptura
- **Éxito**: Identifica degradación graceful
- **Indica**: Límites máximos del sistema

---

## Mejores Prácticas

### Para Pruebas Unitarias

1. **Aislamiento**: Cada test usa contexto independiente
2. **Nomenclatura**: Patrón `MethodName_Condition_ExpectedBehavior`
3. **Limpieza**: Usa `using` para liberar recursos
4. **Datos**: Usa en-memory database para velocidad
5. **Validación**: Verifica tipos y estados, no solo valores

### Para Pruebas k6

1. **Configuración**: Usa variables de entorno para flexibilidad
2. **Pacing**: Respeta límites de API (throttling)
3. **Validaciones**: Usa `check()` para métricas
4. **Logging**: Logs útiles solo para debug
5. **Documentación**: Comenta configuración compleja

### General

1. **Regularidad**: Ejecuta tests frecuentemente
2. **Automatización**: Integra con CI/CD
3. **Monitoreo**: Revisa reportes regularmente
4. **Iteración**: Mejora tests basado en resultados
5. **Documentación**: Mantén actualizada la documentación

---

## Troubleshooting

### Pruebas Unitarias

**Problema**: Tests fallan con `DbUpdateException`
```
Solución: Asegúrate que todas las referencias de Foreign Key existen
```

**Problema**: Tests ignoran configuración JWT
```
Solución: Verifica que CreateTestConfiguration() está siendo llamado
```

### Pruebas k6

**Problema**: Error de conexión
```bash
# Verifica que la API está corriendo
curl http://localhost:5142/api/usuarios

# Verifica puerto correcto
# Por defecto: 5142, alternativo: 7142
```

**Problema**: Disponibilidad baja
```
Causa: API demasiado lenta o saturada
Solución: Revisa logs de API y optimiza endpoints
```

**Problema**: k6 no reconocido como comando
```bash
# Reinstala k6
# Verifica PATH de Windows/Linux
k6 --version  # Debe retornar versión
```

---

## Recursos Adicionales

### Documentación Oficial
- **xUnit**: https://xunit.net/
- **k6**: https://k6.io/docs/
- **.NET Testing**: https://learn.microsoft.com/en-us/dotnet/core/testing/

### Comandos Útiles

```bash
# Pruebas unitarias
dotnet test --filter "SecurityTests"          # Ejecutar test específico
dotnet test --verbosity detailed              # Salida detallada

# Pruebas k6
k6 run --duration 5m k6-load-test-login.js   # Modificar duración
k6 run --vus 100 --duration 1m k6-smoke-test-login.js  # Modificar usuarios
k6 stats k6-reports/load-results.json        # Analizar resultados
```

---

## Conclusión

Este proyecto implementa una estrategia de testing multinivel:
- **Pruebas Unitarias**: Validación de lógica y seguridad
- **Pruebas de Carga**: Verificación de rendimiento y escalabilidad
- **CI/CD**: Automatización continua de validaciones

Todos los tests están documentados, son reproducibles y están integrados en el pipeline de desarrollo.

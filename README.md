# Proyecto de Gestión de Eventos - IA

Sistema de gestión de eventos desarrollado en **.NET 9.0** con **MySQL** y pruebas de carga con **k6**.

## 📖 Documentación

### Inicio Rápido
- **[LOCAL-SETUP.md](./LOCAL-SETUP.md)** ⭐ **EMPIEZA AQUÍ**
  - Configuración completa para Visual Studio 2022
  - Instalación de MySQL y k6
  - Ejecución de pruebas en computadora personal
  - Troubleshooting y checklist

### Documentación Técnica
- **[TESTING.md](./TESTING.md)**
  - Pruebas unitarias (.NET/xUnit) con 6 archivos de test
  - Pruebas de carga y rendimiento (k6)
  - Integración con CI/CD
  - Análisis de reportes
  
- **[WORKFLOW-README.md](./WORKFLOW-README.md)**
  - Arquitectura de GitHub Actions
  - Jobs automáticos y manuales
  - v2.1 con Docker Compose (borrador experimental)
  - Métricas y thresholds

- **[DATABASE-SETUP.md](./ia-proyecto-eventos/DATABASE-SETUP.md)**
  - Inicialización de base de datos
  - Scripts SQL
  - Migraciones y backup

---

## 🚀 Inicio Rápido (5 minutos)

### Requisitos Mínimos
- Visual Studio 2022
- .NET SDK 9.0
- MySQL Server 8.0
- k6 (para pruebas de carga)

### Pasos

1. **Clonar/descargar el proyecto**
2. **Crear base de datos**:
   ```bash
   mysql -u root -p < ia-proyecto-eventos\scripts\init-db.sql
   ```
3. **Abrir en Visual Studio** y presionar **F5**
4. **API disponible en**: http://localhost:5142

👉 **Instrucciones detalladas**: Ver [LOCAL-SETUP.md](./LOCAL-SETUP.md)

---

## 📁 Estructura del Proyecto

```
iaproyectosprincipal/
├── ia-proyecto-eventos/              # API .NET
│   ├── Controllers/                  # Endpoints REST
│   ├── Models/                       # Modelos de datos
│   ├── appsettings.json             # Configuración
│   └── scripts/init-db.sql          # Script de BD
│
├── IaProyectoEventos.Tests/         # Pruebas
│   ├── *ControllerTests.cs          # 6 archivos de tests unitarios
│   ├── SecurityTests.cs             # Tests de seguridad
│   └── scripts/                     # Scripts k6
│       ├── k6-smoke-test-login.js
│       ├── k6-load-test-login.js
│       └── k6-stress-test-login.js
│
├── .github/workflows/               # CI/CD
│   └── k6-load-testing.yml
│
├── LOCAL-SETUP.md                   # Este documento
├── TESTING.md                       # Documentación de tests
└── README.md                        # Este archivo
```

---

## 🧪 Tipos de Pruebas

### Pruebas Unitarias (xUnit)
```bash
# En Visual Studio: Test → Run All Tests
# O terminal:
dotnet test IaProyectoEventos.Tests/IaProyectoEventos.Tests.csproj
```

**6 archivos de test**, 30+ casos:
- EventosControllerTests
- UsuariosControllerTests
- PersonasControllerTests
- RegistroAsistenciasControllerTests
- TipoEventosControllerTests
- SecurityTests (JWT, SQL injection)

### Pruebas de Carga (k6)
```bash
# Smoke test (rápido)
cd IaProyectoEventos.Tests/scripts
./run-k6-tests.bat smoke

# Load test (~4-5 min)
./run-k6-tests.bat load

# Stress test (~5-6 min)
./run-k6-tests.bat stress
```

---

## 🔧 Requisitos

### Software
| Componente | Versión | Descripción |
|-----------|---------|-------------|
| Visual Studio | 2022 | IDE (Community, Pro, Enterprise) |
| .NET SDK | 9.0.x | Runtime y herramientas |
| MySQL | 8.0.x | Base de datos relacional |
| k6 | Latest | Pruebas de carga |
| Docker | Latest | (Opcional) Para ejecutar servicios |

### Hardware
- RAM: 8 GB mínimo (16 GB recomendado)
- Disco: 5 GB libres
- CPU: Dual-core o superior

---

## 💻 Configuración de Visual Studio 2022

### Workloads Necesarios
- ✅ ASP.NET and web development
- ✅ .NET desktop development  
- ✅ Data storage and processing

### Extensions Recomendadas
- REST Client
- MySQL Tools
- Docker
- Thunder Client (para API testing)

---

## 🔌 Conexión a Base de Datos

**Archivo**: `ia-proyecto-eventos/appsettings.json`

```json
{
  "ConnectionStrings": {
    "MySQLConnection": "server=localhost;port=3306;database=ia_proyecto_eventos;user=root;password=mysql;"
  }
}
```

Ajustar según tu configuración local de MySQL.

---

## 📊 Ejecución de API

### Desde Visual Studio
- Presionar **F5** o **Debug → Start Debugging**
- API estará en: http://localhost:5142

### Desde Terminal
```bash
cd ia-proyecto-eventos
dotnet run --configuration Release
```

### Verificar que funciona
```bash
curl http://localhost:5142/api/usuarios
# Debería retornar JSON array
```

---

## 📈 Métricas y Reportes

### k6 genera reportes en JSON
```
IaProyectoEventos.Tests/scripts/k6-reports/
├── smoke-results.json
├── smoke-report.html
├── load-results.json
├── load-report.html
├── stress-results.json
└── stress-report.html
```

### Abrir reportes
```bash
# Windows
start k6-reports/smoke-report.html

# Linux
xdg-open k6-reports/smoke-report.html

# Mac
open k6-reports/smoke-report.html
```

---

## ⚙️ Configuración de k6

Variables de entorno en scripts:
```javascript
const BASE_URL = __ENV.BASE_URL || 'http://localhost:5142';
const TEST_USERNAME = __ENV.TEST_USERNAME || 'admin';
const TEST_PASSWORD = __ENV.TEST_PASSWORD || 'password123';
```

Personalizar ejecución:
```bash
k6 run script.js \
  -e BASE_URL=http://localhost:5142 \
  -e TEST_USERNAME=myuser \
  -e TEST_PASSWORD=mypass
```

---

## 🐛 Troubleshooting

### Error: MySQL no conecta
1. Verificar que MySQL está corriendo: `mysqladmin -u root -p ping`
2. Verificar credenciales en `appsettings.json`
3. Ver [LOCAL-SETUP.md - Troubleshooting](./LOCAL-SETUP.md#-troubleshooting)

### Error: Puerto 5142 en uso
1. Cambiar puerto en `appsettings.json`
2. O matar proceso: `taskkill /PID <PID> /F`

### Error: k6 not found
1. Instalar desde https://k6.io/docs/getting-started/installation
2. Agregar a PATH del sistema
3. Reiniciar terminal

👉 **Más problemas**: Ver [LOCAL-SETUP.md - Troubleshooting](./LOCAL-SETUP.md#-troubleshooting)

---

## 📚 Documentación Completa

| Documento | Contenido |
|-----------|-----------|
| **[LOCAL-SETUP.md](./LOCAL-SETUP.md)** | Configuración local Visual Studio + MySQL + k6 |
| **[TESTING.md](./TESTING.md)** | Tests unitarios, k6, CI/CD, análisis de reportes |
| **[WORKFLOW-README.md](./WORKFLOW-README.md)** | GitHub Actions, arquitectura, v2.1 Docker |
| **[DATABASE-SETUP.md](./ia-proyecto-eventos/DATABASE-SETUP.md)** | BD, scripts SQL, migraciones |

---

## 🔐 Seguridad

### Tests de Seguridad Incluidos
- ✅ JWT Token validation
- ✅ SQL Injection prevention
- ✅ Input validation
- ✅ Authentication/Authorization

Ver `IaProyectoEventos.Tests/SecurityTests.cs`

---

## 🚀 CI/CD - GitHub Actions

### Ejecución Automática
- **Push** a main/develop → Smoke test automático
- **Pull Request** → Smoke test automático
- **Manual**: Actions → Run Workflow → Seleccionar test type

### Status
- ✅ v2.0: Funcionando (con limitaciones de BD)
- ⏳ v2.1: Docker Compose (experimental, listo para probar)

Ver [WORKFLOW-README.md](./WORKFLOW-README.md)

---

## 📞 Contacto y Soporte

Si encuentras problemas:
1. Revisar [LOCAL-SETUP.md - Troubleshooting](./LOCAL-SETUP.md#-troubleshooting)
2. Verificar [TESTING.md](./TESTING.md)
3. Ejecutar checklist: [LOCAL-SETUP.md - Checklist](./LOCAL-SETUP.md#-checklist-de-configuración)

---

## 📝 Licencia

Proyecto educativo - Actividad IA UNIR

---

**Última actualización**: Diciembre 2024

Documentación completa en [LOCAL-SETUP.md](./LOCAL-SETUP.md)

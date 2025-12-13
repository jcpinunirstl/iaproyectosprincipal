# Configuración Local - Visual Studio 2022 + MySQL + k6

Guía completa para ejecutar y probar la aplicación en tu computadora personal.

---

## 📋 Requisitos Previos

### Hardware Mínimo
- **RAM**: 8 GB (recomendado 16 GB)
- **Disco**: 5 GB libres
- **Procesador**: Intel/AMD dual-core o superior

### Software Requerido

#### 1. Visual Studio 2022
- **Versión**: Community, Professional o Enterprise
- **Descarga**: https://visualstudio.microsoft.com/downloads/
- **Workloads necesarios**:
  - ✅ ASP.NET and web development
  - ✅ .NET desktop development
  - ✅ Data storage and processing

#### 2. .NET SDK 9.0
- **Versión**: .NET 9.0.x (LTS)
- **Descarga**: https://dotnet.microsoft.com/download/dotnet/9.0
- **Verificación**:
  ```bash
  dotnet --version
  ```
  Debe mostrar: `9.0.x`

#### 3. MySQL Server 8.0
- **Versión**: MySQL Community Server 8.0.x
- **Descarga**: https://dev.mysql.com/downloads/mysql/
- **Instalación**:
  - Windows: MySQL Installer (recomendado)
  - Configurar puerto: `3306` (por defecto)
  - Root password: Anotar para después
  - Habilitar MySQL Server como Windows Service

#### 4. Docker Desktop (Opcional pero Recomendado)
- **Versión**: Latest stable
- **Descarga**: https://www.docker.com/products/docker-desktop
- **Alternativa**: Docker sin Desktop (CLI solo)
- **Beneficio**: Ejecutar MySQL en contenedor sin instalación nativa

#### 5. k6 (Grafana k6)
- **Versión**: Latest stable
- **Descarga**: https://k6.io/docs/getting-started/installation
- **Instalación Windows**:
  ```bash
  choco install k6          # Si tienes Chocolatey
  # O descargar MSI desde sitio oficial
  ```
- **Verificación**:
  ```bash
  k6 version
  ```

#### 6. Git (Opcional)
- **Versión**: Latest
- **Descarga**: https://git-scm.com/download/win
- **Uso**: Clonar repositorio y gestionar versiones

---

## 🗄️ Configuración de Base de Datos

### Opción A: MySQL Server Instalado Localmente

#### Paso 1: Verificar MySQL está corriendo
```bash
# Windows - Revisar Services
# Buscar "MySQL80" o "MySQL Server" en Services (services.msc)

# O desde CMD:
mysqladmin -u root -p ping
# Debería responder: mysqld is alive
```

#### Paso 2: Crear Base de Datos
```bash
# Conectarse como root
mysql -u root -p
# Ingresar password del root

# En MySQL CLI:
CREATE DATABASE ia_proyecto_eventos;

# Crear usuario para la aplicación:
CREATE USER 'iauser'@'localhost' IDENTIFIED BY 'iapassword';
GRANT ALL PRIVILEGES ON ia_proyecto_eventos.* TO 'iauser'@'localhost';
FLUSH PRIVILEGES;

# Salir
EXIT;
```

#### Paso 3: Ejecutar Script de Inicialización
```bash
# Desde cmd o PowerShell, en carpeta del proyecto:
cd ia-proyecto-eventos\scripts

# Windows
mysql -u root -p ia_proyecto_eventos < init-db.sql

# Ingresar password del root cuando pida
```

**Resultado esperado**: Se crean todas las tablas y se insertan datos iniciales.

### Opción B: MySQL en Docker (Recomendado)

#### Paso 1: Iniciar Docker Desktop
- Abrir Docker Desktop
- Esperar a que aparezca "Docker Desktop is running"

#### Paso 2: Ejecutar Docker Compose
```bash
cd ia-proyecto-eventos

# Iniciar servicios (MySQL + API + Frontend)
docker-compose up -d

# Verificar que los contenedores están corriendo
docker-compose ps
```

**Resultado esperado**:
```
CONTAINER ID   IMAGE              STATUS
xxx123         mysql:8.0          Up 2 minutes
yyy456         ia-proyecto-api    Up 1 minute
zzz789         nginx:alpine       Up 1 minute
```

#### Paso 3: Verificar MySQL
```bash
# Conectar a MySQL en contenedor
docker exec -it ia-proyecto-mysql mysql -u root -prootpassword ia_proyecto_eventos

# En MySQL CLI verificar tablas:
SHOW TABLES;

# Debería mostrar: usuarios, eventos, personas, tipo_eventos, registro_asistencias
```

---

## 🔌 Configuración de la Aplicación

### Paso 1: Abrir en Visual Studio 2022

1. **Iniciar Visual Studio 2022**
2. **File** → **Open** → **Folder**
3. Seleccionar carpeta del proyecto: `iaproyectosprincipal`
4. Esperar a que cargue (puede tomar 1-2 minutos)

### Paso 2: Restaurar Dependencias

```bash
# En Visual Studio - Package Manager Console:
dotnet restore ia-proyecto-eventos/IaProyectoEventos.csproj
dotnet restore IaProyectoEventos.Tests/IaProyectoEventos.Tests.csproj
```

O usando UI:
- **Build** → **Clean Solution**
- **Build** → **Rebuild Solution**

### Paso 3: Verificar Connection String

**Archivo**: `ia-proyecto-eventos/appsettings.json`

```json
{
  "ConnectionStrings": {
    "MySQLConnection": "server=localhost;port=3306;database=ia_proyecto_eventos;user=root;password=mysql;"
  }
}
```

**Reemplazar** según tu configuración:
- `server=localhost` → Cambiar si MySQL no está local
- `port=3306` → Cambiar si usas puerto diferente
- `user=root` → Tu usuario MySQL
- `password=mysql` → Tu password MySQL

### Paso 4: Ejecutar Migraciones (si aplica)

Si el proyecto usa EF Core Migrations:
```bash
# Package Manager Console
Update-Database

# O CLI
dotnet ef database update
```

---

## ▶️ Ejecutar la Aplicación

### Desde Visual Studio

1. **Seleccionar proyectos de inicio**:
   - Right-click en solución → **Properties**
   - **Startup Project** → Seleccionar `IaProyectoEventos`

2. **Presionar F5** o **Debug** → **Start Debugging**

3. **Esperar** a que aparezca la ventana del navegador

4. **API estará disponible en**: `http://localhost:5142`

   **Verificar endpoints**:
   ```bash
   curl http://localhost:5142/api/usuarios
   # Debería retornar JSON array
   ```

### Desde Terminal (Alternativa)

```bash
cd ia-proyecto-eventos

# Ejecutar en modo Debug
dotnet run --configuration Debug

# O Release (más rápido pero menos información de debug)
dotnet run --configuration Release

# API estará en http://localhost:5142
```

**Parar**: Presionar `Ctrl+C`

---

## 🧪 Ejecutar Pruebas Unitarias

### En Visual Studio

1. **Test** → **Test Explorer**
2. Click en **Run All Tests**
3. Esperar resultados

### Desde Terminal

```bash
# Ejecutar todos los tests
dotnet test IaProyectoEventos.Tests/IaProyectoEventos.Tests.csproj

# Tests específicos
dotnet test IaProyectoEventos.Tests/IaProyectoEventos.Tests.csproj --filter "SecurityTests"

# Con cobertura
dotnet test IaProyectoEventos.Tests/IaProyectoEventos.Tests.csproj /p:CollectCoverage=true

# Modo verbose
dotnet test IaProyectoEventos.Tests/IaProyectoEventos.Tests.csproj --verbosity detailed
```

---

## 📊 Ejecutar Pruebas de Carga con k6

### Requisito: API debe estar ejecutándose
```bash
# Terminal 1: Ejecutar API
cd ia-proyecto-eventos
dotnet run --configuration Release

# Terminal 2: Ejecutar k6 tests (esperar a que API esté lista)
```

### Opción A: Scripts Proporcionados

#### Windows (desde PowerShell)
```bash
cd IaProyectoEventos.Tests\scripts

# Smoke test (rápido, ~15 segundos)
.\run-k6-tests.bat smoke

# Load test (~4-5 minutos)
.\run-k6-tests.bat load

# Stress test (~5-6 minutos)
.\run-k6-tests.bat stress

# Todos (~15 minutos)
.\run-k6-tests.bat all
```

#### Linux/Mac
```bash
cd IaProyectoEventos.Tests/scripts

chmod +x run-k6-tests.sh

./run-k6-tests.sh smoke
./run-k6-tests.sh load
./run-k6-tests.sh stress
./run-k6-tests.sh all
```

### Opción B: Comandos Directos k6

#### Smoke Test
```bash
k6 run `
  --vus 1 `
  --duration 10s `
  --summary-export=smoke-results.json `
  IaProyectoEventos.Tests/scripts/k6-smoke-test-login.js `
  -e BASE_URL=http://localhost:5142 `
  -e TEST_USERNAME=admin `
  -e TEST_PASSWORD=password123
```

#### Load Test
```bash
k6 run `
  --summary-export=load-results.json `
  IaProyectoEventos.Tests/scripts/k6-load-test-login.js `
  -e BASE_URL=http://localhost:5142 `
  -e TEST_USERNAME=admin `
  -e TEST_PASSWORD=password123
```

#### Stress Test
```bash
k6 run `
  --summary-export=stress-results.json `
  IaProyectoEventos.Tests/scripts/k6-stress-test-login.js `
  -e BASE_URL=http://localhost:5142 `
  -e TEST_USERNAME=admin `
  -e TEST_PASSWORD=password123
```

### Visualizar Reportes

Los reportes se generan en `k6-reports/`:
- `smoke-report.html` - Abrir en navegador
- `load-report.html`
- `stress-report.html`

```bash
# Windows
start k6-reports/smoke-report.html

# Linux
xdg-open k6-reports/smoke-report.html

# Mac
open k6-reports/smoke-report.html
```

---

## 🐛 Troubleshooting

### Problema: "Error connecting to MySQL"
```
ConnectionString: server=localhost;port=3306;...
Error: Unable to connect to any of the specified MySQL hosts
```

**Soluciones**:
1. Verificar MySQL está corriendo: `mysqladmin -u root -p ping`
2. Verificar puerto: `netstat -ano | findstr :3306` (Windows)
3. Verificar credenciales en `appsettings.json`
4. Reiniciar MySQL Service

### Problema: "Port 5142 already in use"
```
System.Net.Sockets.SocketException: Only one usage of each socket address
```

**Soluciones**:
1. Buscar proceso en puerto: `netstat -ano | findstr :5142`
2. Matar proceso: `taskkill /PID <PID> /F`
3. Cambiar puerto en `appsettings.json`: `"Urls": "http://localhost:5143"`

### Problema: "k6 not found" o "command not recognized"
```
'k6' is not recognized as an internal or external command
```

**Soluciones**:
1. Verificar instalación: `k6 version`
2. Agregar k6 a PATH (Windows):
   - **System Properties** → **Environment Variables**
   - Buscar path de k6 installation
   - Agregar a PATH
3. Reiniciar terminal/PowerShell

### Problema: "Database already exists"
```
Error: Database ia_proyecto_eventos already exists
```

**Soluciones**:
1. Eliminar y recrear:
   ```sql
   DROP DATABASE ia_proyecto_eventos;
   CREATE DATABASE ia_proyecto_eventos;
   ```
2. O ejecutar solo script de datos:
   ```bash
   mysql -u root -p ia_proyecto_eventos < init-db-data.sql
   ```

### Problema: ".NET SDK not found"
```
Could not find .NET SDK
```

**Soluciones**:
1. Verificar instalación: `dotnet --version`
2. Descargar .NET 9.0 desde: https://dotnet.microsoft.com/download/dotnet/9.0
3. Reiniciar Visual Studio después de instalar

### Problema: "k6 tests timeout"
```
Timeout waiting for API to respond
```

**Soluciones**:
1. Verificar API está realmente ejecutándose: `curl http://localhost:5142/api/usuarios`
2. Aumentar timeout en k6 script:
   ```javascript
   let response = http.get('http://localhost:5142/api/usuarios', {
     timeout: '30s'  // Aumentar de 10s a 30s
   });
   ```
3. Aumentar timeout en script shell: `timeout 120 bash -c ...`

---

## 📁 Estructura de Directorios

```
iaproyectosprincipal/
├── ia-proyecto-eventos/                    # API .NET
│   ├── IaProyectoEventos.csproj
│   ├── appsettings.json                    # Conexión a BD
│   ├── docker-compose.yml                  # Servicios en contenedores
│   ├── scripts/
│   │   └── init-db.sql                     # Script de BD
│   ├── Controllers/                        # API endpoints
│   ├── Models/                             # Modelos de datos
│   └── Program.cs                          # Entry point
│
├── IaProyectoEventos.Tests/                # Pruebas
│   ├── IaProyectoEventos.Tests.csproj
│   ├── *ControllerTests.cs                 # Tests unitarios
│   ├── SecurityTests.cs
│   └── scripts/
│       ├── k6-smoke-test-login.js          # k6 Smoke test
│       ├── k6-load-test-login.js           # k6 Load test
│       ├── k6-stress-test-login.js         # k6 Stress test
│       ├── run-k6-tests.bat                # Windows script
│       └── run-k6-tests.sh                 # Linux/Mac script
│
├── .github/
│   └── workflows/
│       └── k6-load-testing.yml             # CI/CD GitHub Actions
│
├── TESTING.md                              # Documentación de tests
├── WORKFLOW-README.md                      # Documentación de CI/CD
├── LOCAL-SETUP.md                          # Este archivo
└── README.md                               # Documentación general
```

---

## ✅ Checklist de Configuración

- [ ] Visual Studio 2022 instalado
- [ ] .NET SDK 9.0.x instalado y verificado
- [ ] MySQL Server 8.0 instalado y corriendo
- [ ] Base de datos `ia_proyecto_eventos` creada
- [ ] Script `init-db.sql` ejecutado
- [ ] k6 instalado y en PATH
- [ ] Repositorio clonado o descargado
- [ ] `appsettings.json` configurado con credenciales correctas
- [ ] API inicia sin errores en Visual Studio
- [ ] Endpoints responden: `http://localhost:5142/api/usuarios`
- [ ] Tests unitarios ejecutan sin errores
- [ ] k6 smoke test ejecuta exitosamente

---

## 🚀 Flujo Típico de Desarrollo

### Día a día:

```bash
# 1. Abrir proyecto en Visual Studio 2022
# File → Open → Folder

# 2. Asegurar BD está ejecutándose (si no usas Docker)
mysqladmin -u root -p ping

# 3. O iniciar con Docker Compose
cd ia-proyecto-eventos
docker-compose up -d

# 4. Ejecutar API (F5 o Debug)
# En Visual Studio: Press F5

# 5. Ejecutar tests unitarios
# En Visual Studio: Test → Run All Tests

# 6. Probar con k6 (en otra terminal)
cd IaProyectoEventos.Tests/scripts
./run-k6-tests.bat smoke   # Windows
./run-k6-tests.sh smoke    # Linux/Mac

# 7. Revisar resultados
# Abrir k6-reports/smoke-report.html en navegador
```

---

## 📚 Recursos Adicionales

### Documentación del Proyecto
- **TESTING.md** - Guía completa de tests unitarios y k6
- **WORKFLOW-README.md** - Documentación de CI/CD y GitHub Actions
- **DATABASE-SETUP.md** - Detalles de configuración de BD

### Links Oficiales
- [Visual Studio 2022 Docs](https://learn.microsoft.com/en-us/visualstudio/)
- [.NET 9.0 Docs](https://learn.microsoft.com/en-us/dotnet/)
- [MySQL Docs](https://dev.mysql.com/doc/)
- [k6 Docs](https://k6.io/docs/)
- [Docker Compose](https://docs.docker.com/compose/)

### Comandos de Referencia
```bash
# .NET
dotnet --version
dotnet restore
dotnet build
dotnet run
dotnet test
dotnet publish

# MySQL
mysql -u root -p
mysqladmin -u root -p ping
mysqldump -u root -p db_name > backup.sql

# k6
k6 version
k6 run script.js
k6 run script.js -e VAR=value

# Docker
docker-compose up -d
docker-compose down
docker-compose ps
docker logs container_name
```

---

## 💡 Consejos y Mejores Prácticas

### Desarrollo Eficiente
- ✅ Usar Visual Studio Code/Rider para debugging rápido
- ✅ Ejecutar API en Release mode si vas a probar performance
- ✅ Usar Docker Compose para no contaminar tu máquina
- ✅ Mantener terminal separada para API vs k6

### Testing
- ✅ Ejecutar tests antes de hacer commit
- ✅ Usar "Run All Tests" regularmente
- ✅ Ejecutar k6 smoke test como sanity check antes de load/stress
- ✅ Guardar reportes de k6 para comparar tendencias

### BD
- ✅ Hacer backup antes de cambios grandes
- ✅ Usar datos de prueba consistentes
- ✅ No usar BD de producción en desarrollo
- ✅ Verificar connection pooling en release mode

---

## 🆘 Soporte

Si encuentras problemas:

1. **Verificar requisitos**: Ejecutar checklist de arriba
2. **Revisar logs**: Visual Studio Output window
3. **Buscar en documentación**: TESTING.md, WORKFLOW-README.md
4. **Reproducir problema**: En terminal separada, paso a paso
5. **Documentar error**: Incluir logs completos, versiones software, SO


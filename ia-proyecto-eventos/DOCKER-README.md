# 🐳 Guía de Despliegue con Docker

## ✨ Ventajas de Usar Docker

Con Docker **NO necesitas instalar**:
- ❌ .NET SDK 9.0
- ❌ MySQL Server
- ❌ Node.js o servidor web
- ❌ Configurar variables de entorno manualmente

**Solo necesitas**: Docker Desktop instalado ✅

## 📦 Requisitos Previos

**ÚNICAMENTE necesitas tener instalado:**

1. **Docker Desktop** 
   - Windows/Mac: [Descargar Docker Desktop](https://www.docker.com/products/docker-desktop)
   - Linux: Instalar Docker Engine + Docker Compose

2. **Verificar instalación:**
```bash
docker --version
docker-compose --version
```

## 🚀 Despliegue Completo en 2 Pasos

### Paso 1: Clonar/Descargar el proyecto

```bash
cd ia-proyecto-eventos
```

### Paso 2: Ejecutar Docker Compose

```bash
docker-compose up --build
```

**¡Eso es todo!** 🎉 Docker se encargará de:
- ✅ Construir la imagen de la API .NET
- ✅ Descargar y configurar MySQL
- ✅ Configurar Nginx para el frontend
- ✅ Crear las tablas de la base de datos automáticamente
- ✅ Cargar datos de prueba (usuarios, eventos, etc.)
- ✅ Conectar todos los servicios

### Acceso a la Aplicación

**Espera 30-60 segundos** para que todos los servicios inicien, luego accede a:

- **🌐 Frontend**: http://localhost:8080
- **🔌 API**: http://localhost:5142
- **📚 Swagger**: http://localhost:5142/swagger
- **💾 MySQL**: localhost:3307

### Modo Detached (segundo plano)

Si prefieres que los contenedores corran en segundo plano:

```bash
docker-compose up -d --build
```

## 🗄️ Gestión de Base de Datos

### Exportar Base de Datos

**Windows:**
```batch
.\scripts\export-database.bat
```

**Linux/Mac:**
```bash
chmod +x ./scripts/export-database.sh
./scripts/export-database.sh
```

El backup se guardará en `./backups/` con timestamp.

### Importar Base de Datos

**Windows:**
```batch
.\scripts\import-database.bat ruta\al\archivo.sql
```

### Acceder a MySQL directamente

```bash
docker exec -it ia-proyecto-mysql mysql -uiauser -piapassword ia_proyecto_eventos
```

## 🛠️ Comandos Útiles

### Ver logs de los contenedores

```bash
# Todos los servicios
docker-compose logs -f

# Solo API
docker-compose logs -f api

# Solo MySQL
docker-compose logs -f mysql
```

### Reiniciar servicios

```bash
# Reiniciar todo
docker-compose restart

# Reiniciar solo API
docker-compose restart api
```

### Detener contenedores

```bash
docker-compose down
```

### Detener y eliminar volúmenes (datos)

```bash
docker-compose down -v
```

### Reconstruir solo un servicio

```bash
docker-compose up --build api
```

## 👥 Credenciales de Acceso

### Usuarios Pre-configurados

El sistema viene con **3 usuarios de prueba** listos para usar:

| Usuario | Contraseña | Rol | Descripción |
|---------|-----------|-----|-------------|
| `admin` | `admin123` | Administrador | Acceso completo al sistema |
| `usuario1` | `admin123` | Usuario | Puede crear y gestionar eventos |
| `usuario2` | `admin123` | Usuario | Puede crear y gestionar eventos |

### Datos de Prueba Pre-cargados

El script `scripts/init-db.sql` carga automáticamente:
- ✅ **5 Tipos de Eventos**: Conferencia, Taller, Seminario, Networking, Social
- ✅ **3 Usuarios**: Con credenciales funcionales (ver tabla arriba)
- ✅ **5 Personas**: Para registro de asistencias
- ✅ **4 Eventos de Ejemplo**: Configurados y listos
- ✅ **5 Registros de Asistencia**: Datos de muestra

### Primera Vez en el Sistema

1. Abre http://localhost:8080
2. Haz clic en **"Iniciar Sesión"**
3. Usa: `admin` / `admin123`
4. ¡Ya tienes acceso completo! 🎉

## 🔧 Configuración

### Variables de Entorno

Puedes modificar las credenciales de MySQL en `docker-compose.yml`:

```yaml
environment:
  MYSQL_ROOT_PASSWORD: rootpassword
  MYSQL_DATABASE: ia_proyecto_eventos
  MYSQL_USER: iauser
  MYSQL_PASSWORD: iapassword
```

### Puertos

Si necesitas cambiar los puertos, modifica en `docker-compose.yml`:

```yaml
ports:
  - "8080:80"     # Frontend (nginx)
  - "5142:8080"   # API
  - "3307:3306"   # MySQL
```

## 🐛 Solución de Problemas

### La API no puede conectarse a MySQL

Espera a que MySQL esté completamente iniciado. El healthcheck debería manejarlo, pero puedes verificar:

```bash
docker-compose logs mysql
```

### Error al aplicar migraciones

```bash
docker-compose down -v
docker-compose up --build
```

### Puerto ya en uso

Cambia el puerto en `docker-compose.yml` o detén el servicio que está usando el puerto:

```bash
# Windows
netstat -ano | findstr :8080

# Linux/Mac  
lsof -i :8080
```

## 📁 Estructura de Archivos Docker

```
├── Dockerfile              # Imagen de la API
├── docker-compose.yml      # Orquestación de servicios
├── .dockerignore          # Archivos excluidos de la imagen
├── nginx.conf             # Configuración del proxy Nginx
├── appsettings.Production.json  # Config para producción
└── scripts/
    ├── init-db.sql        # Datos iniciales
    ├── export-database.bat
    ├── export-database.sh
    └── import-database.bat
```

## 🔄 Workflow de Desarrollo

1. **Hacer cambios en el código**
2. **Reconstruir la API**: `docker-compose up --build api`
3. **Ver logs**: `docker-compose logs -f api`
4. **Probar**: http://localhost:8080

## 📝 Notas Importantes

- ✅ Los datos de MySQL persisten en el volumen `mysql_data`
- ✅ Las tablas se crean automáticamente al iniciar MySQL por primera vez
- ✅ El script `init-db.sql` se ejecuta solo la primera vez (al crear el contenedor)
- ✅ El frontend se sirve a través de Nginx con proxy a la API
- ✅ Swagger está disponible también en producción para pruebas
- ✅ No necesitas instalar .NET, MySQL ni ninguna dependencia
- ✅ Todo el sistema funciona de forma aislada en contenedores
- ⚠️ **No se usan migraciones de Entity Framework** - Todo se maneja con SQL directo

## 🎯 Guía Rápida de Inicio

### Para Desarrolladores sin Experiencia en Docker

1. **Instala Docker Desktop** (solo una vez)
   - Descarga de: https://www.docker.com/products/docker-desktop
   - Instala y reinicia tu computadora

2. **Abre una terminal** en la carpeta del proyecto
   ```bash
   cd ia-proyecto-eventos
   ```

3. **Ejecuta un solo comando**
   ```bash
   docker-compose up --build
   ```

4. **Espera 1-2 minutos** (solo la primera vez mientras descarga imágenes)

5. **Abre tu navegador** en http://localhost:8080

6. **Inicia sesión** con `admin` / `admin123`

### Para Detener el Sistema

Presiona `Ctrl + C` en la terminal donde está corriendo

O si lo ejecutaste en modo detached:
```bash
docker-compose down
```

### Para Reiniciar con Datos Limpios

```bash
docker-compose down -v
docker-compose up --build
```

## 🌟 Arquitectura de Contenedores

El proyecto despliega **3 contenedores**:

```
┌─────────────────────────────────────────┐
│  🌐 Frontend (Nginx) - Puerto 8080     │
│  Sirve: HTML, CSS, JS                   │
│  Proxy a API: /api/* → api:8080        │
└─────────────┬───────────────────────────┘
              │
              ▼
┌─────────────────────────────────────────┐
│  🔌 API (.NET 9) - Puerto 5142         │
│  Backend: Controllers, EF Core          │
│  Conecta con: MySQL                     │
└─────────────┬───────────────────────────┘
              │
              ▼
┌─────────────────────────────────────────┐
│  💾 MySQL 8.0 - Puerto 3307            │
│  Base de Datos: ia_proyecto_eventos     │
│  Volumen persistente: mysql_data        │
└─────────────────────────────────────────┘
```

## ⚡ Comandos Más Usados

```bash
# Iniciar todo
docker-compose up -d

# Ver logs en tiempo real
docker-compose logs -f

# Detener todo
docker-compose down

# Reiniciar solo la API
docker-compose restart api

# Ver estado de contenedores
docker-compose ps

# Acceder a MySQL
docker exec -it ia-proyecto-mysql mysql -uiauser -piapassword ia_proyecto_eventos

# Exportar base de datos
.\scripts\export-database.bat  # Windows
./scripts/export-database.sh   # Linux/Mac
```

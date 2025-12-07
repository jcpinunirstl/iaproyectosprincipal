# 🗄️ Configuración de Base de Datos

## Filosofía de Diseño

Este proyecto **NO usa migraciones de Entity Framework Core**. En su lugar, utiliza scripts SQL directos para máxima portabilidad y control.

## ✅ Ventajas de este Enfoque

- ✅ **Portabilidad Total**: El script SQL funciona en cualquier entorno MySQL
- ✅ **Sin Dependencias de EF**: No necesitas Entity Framework tools para desplegar
- ✅ **Control Completo**: Ves exactamente qué se ejecuta en la BD
- ✅ **Fácil Debugging**: Si algo falla, el error es claro en el log de MySQL
- ✅ **Versión Única**: Un solo script SQL controla toda la estructura

## 📁 Estructura de Archivos

```
scripts/
├── init-db.sql          # Script principal (crea tablas + datos)
├── export-database.bat  # Exportar BD (Windows)
├── export-database.sh   # Exportar BD (Linux/Mac)
└── import-database.bat  # Importar BD (Windows)
```

## 🚀 Inicialización de la Base de Datos

### Opción 1: Docker (Automático)

```bash
docker-compose up --build
```

El script `init-db.sql` se ejecuta automáticamente al crear el contenedor por primera vez.

**¿Cómo funciona?**
1. Docker crea el contenedor MySQL
2. MySQL detecta el archivo en `/docker-entrypoint-initdb.d/init-db.sql`
3. Ejecuta el script automáticamente
4. Crea todas las tablas y carga datos iniciales

### Opción 2: Manual (Desarrollo Local)

**Requisitos:**
- MySQL Server instalado y corriendo
- Base de datos `ia_proyecto_eventos` creada

**Pasos:**

1. **Crear la base de datos** (si no existe):
```sql
CREATE DATABASE ia_proyecto_eventos;
```

2. **Ejecutar el script de inicialización**:

```bash
# Windows (PowerShell)
Get-Content scripts\init-db.sql | mysql -u root -p ia_proyecto_eventos

# Windows (CMD)
type scripts\init-db.sql | mysql -u root -p ia_proyecto_eventos

# Linux/Mac
mysql -u root -p ia_proyecto_eventos < scripts/init-db.sql
```

## 📊 Estructura de la Base de Datos

### Tablas Creadas

| Tabla | Descripción | Registros Iniciales |
|-------|-------------|---------------------|
| `TipoEventos` | Tipos de eventos disponibles | 5 tipos |
| `Usuarios` | Usuarios del sistema | 3 usuarios |
| `Personas` | Personas para asistencia | 5 personas |
| `Eventos` | Eventos registrados | 4 eventos |
| `RegistroAsistencias` | Asistencias a eventos | 5 registros |

### Relaciones

```
TipoEventos ─┐
             ├─→ Eventos ──→ RegistroAsistencias
Usuarios ────┘                      ↓
                              Personas
```

## 🔄 Reiniciar la Base de Datos

### En Docker

```bash
# Eliminar volumen y recrear todo
docker-compose down -v
docker-compose up --build
```

**Nota**: Al usar `-v` se eliminan TODOS los datos del volumen MySQL.

### En Local

```bash
# Eliminar base de datos
mysql -u root -p -e "DROP DATABASE IF EXISTS ia_proyecto_eventos;"

# Crear nueva
mysql -u root -p -e "CREATE DATABASE ia_proyecto_eventos;"

# Re-ejecutar script
mysql -u root -p ia_proyecto_eventos < scripts/init-db.sql
```

## ⚠️ Modificaciones al Esquema

### Si Necesitas Cambiar la Estructura

1. **Modifica el archivo `scripts/init-db.sql`**
   - Edita la sección `CREATE TABLE` según necesites
   
2. **Para aplicar cambios en Docker**:
   ```bash
   docker-compose down -v
   docker-compose up --build
   ```

3. **Para aplicar cambios en local**:
   ```bash
   # Opción A: Eliminar y recrear
   mysql -u root -p -e "DROP DATABASE ia_proyecto_eventos;"
   mysql -u root -p -e "CREATE DATABASE ia_proyecto_eventos;"
   mysql -u root -p ia_proyecto_eventos < scripts/init-db.sql
   
   # Opción B: Ejecutar ALTER TABLE manualmente
   mysql -u root -p ia_proyecto_eventos
   > ALTER TABLE Eventos ADD COLUMN NuevaColumna VARCHAR(100);
   ```

## 📤 Exportar la Base de Datos

### Windows
```bash
.\scripts\export-database.bat
```

### Linux/Mac
```bash
chmod +x ./scripts/export-database.sh
./scripts/export-database.sh
```

El backup se guardará en: `./backups/ia_proyecto_eventos_backup_YYYYMMDD_HHMMSS.sql`

## 📥 Importar una Base de Datos

### Windows
```bash
.\scripts\import-database.bat ruta\al\archivo.sql
```

### Linux/Mac
```bash
mysql -u root -p ia_proyecto_eventos < backup.sql
```

## 🔍 Verificar Estado de la Base de Datos

```bash
# Conectarse a MySQL en Docker
docker exec -it ia-proyecto-mysql mysql -uiauser -piapassword ia_proyecto_eventos

# Ver tablas
SHOW TABLES;

# Contar registros
SELECT COUNT(*) FROM Eventos;
SELECT COUNT(*) FROM Usuarios;
```

## 🐛 Solución de Problemas

### Error: "Table doesn't exist"

**Causa**: El script init-db.sql no se ejecutó

**Solución**:
```bash
# Docker
docker-compose down -v
docker-compose up --build

# Local
mysql -u root -p ia_proyecto_eventos < scripts/init-db.sql
```

### Error: "Duplicate entry"

**Causa**: Intentas ejecutar el script cuando las tablas ya tienen datos

**Solución**: El script usa `INSERT IGNORE` que previene duplicados. Si el error persiste:
```bash
# Eliminar datos y reiniciar
TRUNCATE TABLE RegistroAsistencias;
TRUNCATE TABLE Eventos;
TRUNCATE TABLE Personas;
TRUNCATE TABLE Usuarios;
TRUNCATE TABLE TipoEventos;
```

### El script no se ejecuta en Docker

**Causa**: El volumen ya existe de una ejecución anterior

**Solución**:
```bash
# Ver volúmenes
docker volume ls

# Eliminar volumen específico
docker volume rm ia-proyecto-eventos_mysql_data

# O eliminar todos los volúmenes del proyecto
docker-compose down -v
```

## 📝 Buenas Prácticas

1. ✅ **Siempre usa `INSERT IGNORE`** para evitar errores en datos duplicados
2. ✅ **Documenta cambios** en el script SQL con comentarios
3. ✅ **Haz backups** antes de cambios importantes
4. ✅ **Versiona el script** en git para control de cambios
5. ✅ **Prueba cambios** primero en local antes de Docker

## 🔗 Referencias

- [Documentación MySQL](https://dev.mysql.com/doc/)
- [Docker MySQL Init Scripts](https://hub.docker.com/_/mysql)
- [Guía de Docker](./DOCKER-README.md)
- [Solución de Problemas](./TROUBLESHOOTING.md)

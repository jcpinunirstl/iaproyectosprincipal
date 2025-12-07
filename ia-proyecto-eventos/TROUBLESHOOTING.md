# 🔧 Guía de Solución de Problemas

## Error: "Error al obtener eventos"

Este error significa que el frontend no puede comunicarse con el API backend.

### 🔍 Diagnóstico Rápido

#### 1. Verifica que el API esté corriendo

**Abre en tu navegador**: http://localhost:5142/swagger

- ✅ **Si carga Swagger**: El API está funcionando
- ❌ **Si NO carga**: El API no está corriendo

#### 2. Revisa la consola del navegador

1. Presiona **F12** en tu navegador
2. Ve a la pestaña **Console**
3. Busca mensajes como:
   - `API Base URL configurada: ...`
   - `Obteniendo eventos desde: ...`
   - Errores de CORS
   - Errores de red (Failed to fetch)

### 🛠️ Soluciones

#### Solución 1: Iniciar el API Backend

**Si estás en desarrollo local:**

```bash
# En la carpeta raíz del proyecto
dotnet run
```

El API debe iniciar en: http://localhost:5142

**Si estás usando Docker:**

```bash
docker-compose up
```

#### Solución 2: Verificar la URL del API

Abre `web-ia-event/api.js` y verifica que la función `getApiBaseUrl()` esté configurada correctamente.

El frontend detecta automáticamente:
- Puerto 8080 → Usa `/api` (Docker/Nginx)
- Otros casos → Usa `http://localhost:5142/api`

#### Solución 3: Problema de CORS

Si ves en la consola:
```
Access to fetch at 'http://localhost:5142/api/Eventos' from origin '...' has been blocked by CORS policy
```

**Verifica en `Program.cs`:**

```csharp
app.UseCors("AllowAll");
```

Debe estar **ANTES** de:
```csharp
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
```

#### Solución 4: Base de datos no inicializada

Si el API corre pero devuelve error 500, puede ser un problema de base de datos.

**En Docker:**
El script `init-db.sql` se ejecuta automáticamente al crear el contenedor por primera vez.

Si necesitas reinicializar la base de datos:
```bash
# Eliminar volúmenes y recrear
docker-compose down -v
docker-compose up --build
```

**En desarrollo local:**
Ejecuta el script manualmente:
```bash
# Conéctate a MySQL y ejecuta:
mysql -u root -p ia_proyecto_eventos < scripts/init-db.sql
```

### 📋 Checklist de Verificación

- [ ] Docker Desktop está corriendo (si usas Docker)
- [ ] El API está corriendo en http://localhost:5142
- [ ] Swagger carga correctamente en http://localhost:5142/swagger
- [ ] La base de datos MySQL está corriendo
- [ ] Las tablas de la BD están creadas (script init-db.sql ejecutado)
- [ ] El frontend abre correctamente
- [ ] La consola del navegador no muestra errores de CORS

### 🐛 Errores Comunes

#### Error: "Failed to fetch"

**Causa**: El API no está corriendo o la URL es incorrecta

**Solución**: 
1. Inicia el API: `dotnet run` o `docker-compose up`
2. Verifica que cargue: http://localhost:5142/swagger

#### Error: CORS policy

**Causa**: El navegador bloquea la petición por políticas de seguridad

**Solución**: Verifica que `app.UseCors("AllowAll")` esté configurado en `Program.cs`

#### Error: "Cannot read properties of undefined"

**Causa**: El API devuelve un formato inesperado o está vacío

**Solución**: 
1. Revisa los logs del API
2. Verifica que la base de datos tenga datos
3. Ejecuta el script de datos iniciales

### 🚀 Inicio Rápido para Testing

**Opción 1: Docker (Recomendado)**
```bash
# En la raíz del proyecto
docker-compose up --build

# Espera 1-2 minutos y abre:
# http://localhost:8080
```

**Opción 2: Desarrollo Local**
```bash
# Terminal 1: Iniciar API
dotnet run

# Terminal 2: Abrir frontend
# Abre web-ia-event/index.html en tu navegador
# O usa un servidor como Live Server en VS Code
```

### 📞 ¿Aún tienes problemas?

1. **Revisa los logs del API**:
   ```bash
   # Docker
   docker-compose logs api
   
   # Local
   # Los logs aparecen en la terminal donde ejecutaste dotnet run
   ```

2. **Revisa los logs de MySQL**:
   ```bash
   docker-compose logs mysql
   ```

3. **Reinicia todo desde cero**:
   ```bash
   # Docker (elimina volúmenes y recrea todo)
   docker-compose down -v
   docker-compose up --build
   
   # Local (re-ejecuta el script SQL)
   mysql -u root -p ia_proyecto_eventos < scripts/init-db.sql
   dotnet run
   ```

### 🔍 Información de Diagnóstico Útil

Para reportar un problema, incluye:

1. **Salida de la consola del navegador** (F12 → Console)
2. **URL que intentas acceder**
3. **Logs del API** (docker-compose logs api)
4. **Versión de Docker/navegador**
5. **Sistema operativo**

### 📚 Recursos Adicionales

- [Documentación Docker](./DOCKER-README.md)
- [Documentación principal](./README.md)
- Swagger UI: http://localhost:5142/swagger

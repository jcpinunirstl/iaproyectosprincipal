@echo off
REM Script para exportar la base de datos MySQL desde Docker (Windows)

SET CONTAINER_NAME=ia-proyecto-mysql
SET DB_NAME=ia_proyecto_eventos
SET DB_USER=iauser
SET DB_PASSWORD=iapassword
SET BACKUP_DIR=.\backups

REM Obtener timestamp
for /f "tokens=2 delims==" %%I in ('wmic os get localdatetime /value') do set datetime=%%I
set TIMESTAMP=%datetime:~0,8%_%datetime:~8,6%
SET BACKUP_FILE=%BACKUP_DIR%\%DB_NAME%_backup_%TIMESTAMP%.sql

REM Crear directorio de backups si no existe
if not exist %BACKUP_DIR% mkdir %BACKUP_DIR%

echo Exportando base de datos...

REM Exportar con mysqldump desde el contenedor
docker exec %CONTAINER_NAME% mysqldump -u%DB_USER% -p%DB_PASSWORD% %DB_NAME% > %BACKUP_FILE%

if %ERRORLEVEL% EQU 0 (
    echo Base de datos exportada exitosamente a: %BACKUP_FILE%
) else (
    echo Error al exportar la base de datos
    exit /b 1
)

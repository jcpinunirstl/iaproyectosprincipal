@echo off
REM Script para importar la base de datos MySQL a Docker (Windows)

SET CONTAINER_NAME=ia-proyecto-mysql
SET DB_NAME=ia_proyecto_eventos
SET DB_USER=iauser
SET DB_PASSWORD=iapassword

if "%1"=="" (
    echo Uso: import-database.bat ruta_al_archivo.sql
    exit /b 1
)

SET SQL_FILE=%1

echo Importando base de datos desde: %SQL_FILE%

REM Importar SQL al contenedor
docker exec -i %CONTAINER_NAME% mysql -u%DB_USER% -p%DB_PASSWORD% %DB_NAME% < %SQL_FILE%

if %ERRORLEVEL% EQU 0 (
    echo Base de datos importada exitosamente
) else (
    echo Error al importar la base de datos
    exit /b 1
)

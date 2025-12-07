@echo off
REM Script para ejecutar pruebas k6 localmente en Windows
REM Requiere: k6 instalado (https://k6.io/docs/getting-started/installation)

setlocal enabledelayedexpansion

set BASE_URL=http://localhost:7142
set TEST_USERNAME=testuser
set TEST_PASSWORD=TestPassword123!

echo.
echo ============================================
echo K6 Load Testing - Login Tests
echo ============================================
echo.

if "%1"=="" (
    echo Uso: run-k6-tests.bat [smoke^|load^|stress^|all]
    echo.
    echo Ejemplos:
    echo   run-k6-tests.bat smoke     - Ejecutar pruebas de smoke
    echo   run-k6-tests.bat load      - Ejecutar pruebas de carga
    echo   run-k6-tests.bat stress    - Ejecutar pruebas de estres
    echo   run-k6-tests.bat all       - Ejecutar todas las pruebas
    echo.
    goto :eof
)

set TEST_TYPE=%1

echo Verificando que la API está disponible en %BASE_URL%...
powershell -Command "try { $null = Invoke-WebRequest -Uri '%BASE_URL%/api/usuarios' -ErrorAction Stop; Write-Host 'API está disponible'; exit 0 } catch { Write-Host 'API no está disponible'; exit 1 }"
if errorlevel 1 (
    echo ERROR: La API no está disponible en %BASE_URL%
    echo Asegúrate de que la API .NET está corriendo en puerto 7142
    goto :eof
)

mkdir k6-reports 2>nul

echo.
if /i "%TEST_TYPE%"=="smoke" (
    echo Ejecutando Smoke Tests...
    k6 run --vus 1 --duration 10s --summary-export=k6-reports\smoke-results.json k6-smoke-test-login.js -e BASE_URL=%BASE_URL% -e TEST_USERNAME=%TEST_USERNAME% -e TEST_PASSWORD=%TEST_PASSWORD%
    if errorlevel 1 goto :error
    call :generate_report smoke
    call :validate_availability k6-reports\smoke-results.json
    if errorlevel 1 goto :error
    
) else if /i "%TEST_TYPE%"=="load" (
    echo Ejecutando Load Tests (duracion ~4.5 minutos)...
    k6 run --summary-export=k6-reports\load-results.json k6-load-test-login.js -e BASE_URL=%BASE_URL% -e TEST_USERNAME=%TEST_USERNAME% -e TEST_PASSWORD=%TEST_PASSWORD%
    if errorlevel 1 goto :error
    call :generate_report load
    call :validate_availability k6-reports\load-results.json
    if errorlevel 1 goto :error
    
) else if /i "%TEST_TYPE%"=="stress" (
    echo Ejecutando Stress Tests (duracion ~5.5 minutos)...
    k6 run --summary-export=k6-reports\stress-results.json k6-stress-test-login.js -e BASE_URL=%BASE_URL% -e TEST_USERNAME=%TEST_USERNAME% -e TEST_PASSWORD=%TEST_PASSWORD%
    call :generate_report stress
    
) else if /i "%TEST_TYPE%"=="all" (
    echo Ejecutando todas las pruebas...
    echo.
    echo [1/3] Smoke Tests...
    k6 run --vus 1 --duration 10s --summary-export=k6-reports\smoke-results.json k6-smoke-test-login.js -e BASE_URL=%BASE_URL% -e TEST_USERNAME=%TEST_USERNAME% -e TEST_PASSWORD=%TEST_PASSWORD%
    if errorlevel 1 goto :error
    call :generate_report smoke
    call :validate_availability k6-reports\smoke-results.json
    if errorlevel 1 goto :error
    
    echo.
    echo [2/3] Load Tests...
    k6 run --summary-export=k6-reports\load-results.json k6-load-test-login.js -e BASE_URL=%BASE_URL% -e TEST_USERNAME=%TEST_USERNAME% -e TEST_PASSWORD=%TEST_PASSWORD%
    if errorlevel 1 goto :error
    call :generate_report load
    call :validate_availability k6-reports\load-results.json
    if errorlevel 1 goto :error
    
    echo.
    echo [3/3] Stress Tests...
    k6 run --summary-export=k6-reports\stress-results.json k6-stress-test-login.js -e BASE_URL=%BASE_URL% -e TEST_USERNAME=%TEST_USERNAME% -e TEST_PASSWORD=%TEST_PASSWORD%
    call :generate_report stress
    
    echo.
    echo ============================================
    echo Todas las pruebas completadas exitosamente!
    echo ============================================
    echo.
    echo Reportes disponibles en: k6-reports\
    
) else (
    echo ERROR: Tipo de prueba no reconocido: %TEST_TYPE%
    echo Use: smoke, load, stress o all
    goto :eof
)

echo.
echo Reportes disponibles en: k6-reports\
goto :eof

:generate_report
setlocal
set TEST_NAME=%1
echo Generando reporte HTML para %TEST_NAME%...
powershell -NoProfile -Command "& {
  $jsonFile = 'k6-reports\%TEST_NAME%-results.json'
  if (Test-Path $jsonFile) {
    $data = Get-Content $jsonFile | ConvertFrom-Json
    $total = $data.metrics.http_reqs.values.count ?? 0
    $failed = $data.metrics.http_reqs.values.fails ?? 0
    $success = $total - $failed
    $availability = if ($total -gt 0) { [math]::Round(($success / $total) * 100, 2) } else { 0 }
    
    $html = @\"
<!DOCTYPE html>
<html>
<head>
  <meta charset='utf-8'>
  <title>K6 %TEST_NAME% Test Report</title>
  <style>
    body { font-family: Arial, sans-serif; margin: 40px; background: #f5f5f5; }
    .container { background: white; padding: 30px; border-radius: 8px; max-width: 800px; margin: 0 auto; }
    .header { color: #667eea; border-bottom: 2px solid #667eea; padding-bottom: 20px; }
    .status { padding: 15px; margin: 20px 0; border-radius: 5px; }
    .success { background: #d4edda; color: #155724; }
    .error { background: #f8d7da; color: #721c24; }
    table { width: 100%; margin-top: 20px; border-collapse: collapse; }
    th, td { padding: 10px; text-align: left; border-bottom: 1px solid #ddd; }
    th { background: #f8f9fa; font-weight: bold; }
  </style>
</head>
<body>
  <div class='container'>
    <div class='header'><h1>📊 K6 %TEST_NAME% Test Report</h1></div>
    <div class='status \$( if ($availability -ge 99) { 'success' } else { 'error' } )'>
      <strong>Disponibilidad:</strong> \$availability%
    </div>
    <table>
      <tr><th>Métrica</th><th>Valor</th></tr>
      <tr><td>Total de solicitudes</td><td>\$total</td></tr>
      <tr><td>Solicitudes exitosas</td><td>\$success</td></tr>
      <tr><td>Solicitudes fallidas</td><td>\$failed</td></tr>
      <tr><td>Disponibilidad</td><td>\$availability%</td></tr>
    </table>
  </div>
</body>
</html>
\"@
    
    Set-Content -Path "k6-reports\%TEST_NAME%-report.html" -Value $html
    Write-Host '  ✅ Reporte generado: k6-reports\%TEST_NAME%-report.html'
  }
}"
endlocal
goto :eof

:validate_availability
setlocal
set RESULTS_FILE=%1
powershell -NoProfile -Command "& {
  if (Test-Path '%RESULTS_FILE%') {
    $data = Get-Content '%RESULTS_FILE%' | ConvertFrom-Json
    $total = $data.metrics.http_reqs.values.count ?? 0
    $failed = $data.metrics.http_reqs.values.fails ?? 0
    $success = $total - $failed
    $availability = if ($total -gt 0) { [math]::Round(($success / $total) * 100, 2) } else { 0 }
    
    Write-Host '📊 Validación de disponibilidad:'
    Write-Host \"  • Total de solicitudes: \$total\"
    Write-Host \"  • Disponibilidad: \$availability%\"
    
    if ($availability -ge 99) {
      Write-Host '✅ PRUEBA EXITOSA: Disponibilidad >= 99%' -ForegroundColor Green
    } else {
      Write-Host '❌ PRUEBA FALLIDA: Disponibilidad < 99%' -ForegroundColor Red
      exit 1
    }
  }
}"
if errorlevel 1 (
    endlocal
    exit /b 1
)
endlocal
goto :eof

:error
echo.
echo ERROR: Las pruebas fallaron
exit /b 1

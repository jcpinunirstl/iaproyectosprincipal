#!/bin/bash
# Script para ejecutar pruebas k6 localmente en Linux/Mac
# Requiere: k6 instalado (https://k6.io/docs/getting-started/installation)

BASE_URL="${BASE_URL:-http://localhost:5142}"
TEST_USERNAME="${TEST_USERNAME:-testuser}"
TEST_PASSWORD="${TEST_PASSWORD:-TestPassword123!}"

echo ""
echo "============================================"
echo "K6 Load Testing - Login Tests"
echo "============================================"
echo ""

generate_report() {
    local TEST_NAME=$1
    local RESULTS_FILE="k6-reports/${TEST_NAME}-results.json"
    
    if [ ! -f "$RESULTS_FILE" ]; then
        return
    fi
    
    echo "Generando reporte HTML para $TEST_NAME..."
    
    local TOTAL=$(jq '.metrics.http_reqs.values.count // 0' "$RESULTS_FILE")
    local FAILED=$(jq '.metrics.http_reqs.values.fails // 0' "$RESULTS_FILE")
    local SUCCESS=$((TOTAL - FAILED))
    local AVAILABILITY=0
    
    if [ "$TOTAL" -gt 0 ]; then
        AVAILABILITY=$(echo "scale=2; ($SUCCESS / $TOTAL) * 100" | bc)
    fi
    
    local STATUS_CLASS="error"
    if (( $(echo "$AVAILABILITY >= 99" | bc -l) )); then
        STATUS_CLASS="success"
    fi
    
    cat > "k6-reports/${TEST_NAME}-report.html" <<EOF
<!DOCTYPE html>
<html>
<head>
  <meta charset='utf-8'>
  <title>K6 $TEST_NAME Test Report</title>
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
    <div class='header'><h1>📊 K6 $TEST_NAME Test Report</h1></div>
    <div class='status $STATUS_CLASS'>
      <strong>Disponibilidad:</strong> ${AVAILABILITY}%
    </div>
    <table>
      <tr><th>Métrica</th><th>Valor</th></tr>
      <tr><td>Total de solicitudes</td><td>$TOTAL</td></tr>
      <tr><td>Solicitudes exitosas</td><td>$SUCCESS</td></tr>
      <tr><td>Solicitudes fallidas</td><td>$FAILED</td></tr>
      <tr><td>Disponibilidad</td><td>${AVAILABILITY}%</td></tr>
    </table>
  </div>
</body>
</html>
EOF
    
    echo "  ✅ Reporte generado: k6-reports/${TEST_NAME}-report.html"
}

validate_availability() {
    local RESULTS_FILE=$1
    
    if [ ! -f "$RESULTS_FILE" ]; then
        echo "❌ Archivo de resultados no encontrado: $RESULTS_FILE"
        return 1
    fi
    
    local TOTAL=$(jq '.metrics.http_reqs.values.count // 0' "$RESULTS_FILE")
    local FAILED=$(jq '.metrics.http_reqs.values.fails // 0' "$RESULTS_FILE")
    local SUCCESS=$((TOTAL - FAILED))
    local AVAILABILITY=0
    
    if [ "$TOTAL" -gt 0 ]; then
        AVAILABILITY=$(echo "scale=2; ($SUCCESS / $TOTAL) * 100" | bc)
    fi
    
    echo "📊 Validación de disponibilidad:"
    echo "  • Total de solicitudes: $TOTAL"
    echo "  • Disponibilidad: ${AVAILABILITY}%"
    
    if (( $(echo "$AVAILABILITY >= 99" | bc -l) )); then
        echo "✅ PRUEBA EXITOSA: Disponibilidad >= 99%"
        return 0
    else
        echo "❌ PRUEBA FALLIDA: Disponibilidad < 99%"
        return 1
    fi
}

if [ -z "$1" ]; then
    echo "Uso: ./run-k6-tests.sh [smoke|load|stress|all]"
    echo ""
    echo "Ejemplos:"
    echo "  ./run-k6-tests.sh smoke     - Ejecutar pruebas de smoke"
    echo "  ./run-k6-tests.sh load      - Ejecutar pruebas de carga"
    echo "  ./run-k6-tests.sh stress    - Ejecutar pruebas de estres"
    echo "  ./run-k6-tests.sh all       - Ejecutar todas las pruebas"
    echo ""
    exit 0
fi

TEST_TYPE=$1

echo "Verificando que la API está disponible en $BASE_URL..."
if ! curl -s -f "$BASE_URL/api/usuarios" > /dev/null; then
    echo "ERROR: La API no está disponible en $BASE_URL"
    echo "Asegúrate de que la API .NET está corriendo en puerto 5142"
    exit 1
fi
echo "API está disponible"

mkdir -p k6-reports

echo ""
case $TEST_TYPE in
    smoke)
        echo "Ejecutando Smoke Tests..."
        k6 run --vus 1 --duration 10s --summary-export=k6-reports/smoke-results.json k6-smoke-test-login.js \
            -e BASE_URL=$BASE_URL \
            -e TEST_USERNAME=$TEST_USERNAME \
            -e TEST_PASSWORD=$TEST_PASSWORD
        if [ $? -ne 0 ]; then
            exit 1
        fi
        generate_report smoke
        validate_availability "k6-reports/smoke-results.json"
        if [ $? -ne 0 ]; then
            exit 1
        fi
        ;;
    load)
        echo "Ejecutando Load Tests (duracion ~4.5 minutos)..."
        k6 run --summary-export=k6-reports/load-results.json k6-load-test-login.js \
            -e BASE_URL=$BASE_URL \
            -e TEST_USERNAME=$TEST_USERNAME \
            -e TEST_PASSWORD=$TEST_PASSWORD
        if [ $? -ne 0 ]; then
            exit 1
        fi
        generate_report load
        validate_availability "k6-reports/load-results.json"
        if [ $? -ne 0 ]; then
            exit 1
        fi
        ;;
    stress)
        echo "Ejecutando Stress Tests (duracion ~5.5 minutos)..."
        k6 run --summary-export=k6-reports/stress-results.json k6-stress-test-login.js \
            -e BASE_URL=$BASE_URL \
            -e TEST_USERNAME=$TEST_USERNAME \
            -e TEST_PASSWORD=$TEST_PASSWORD
        generate_report stress
        ;;
    all)
        echo "Ejecutando todas las pruebas..."
        echo ""
        
        echo "[1/3] Smoke Tests..."
        k6 run --vus 1 --duration 10s --summary-export=k6-reports/smoke-results.json k6-smoke-test-login.js \
            -e BASE_URL=$BASE_URL \
            -e TEST_USERNAME=$TEST_USERNAME \
            -e TEST_PASSWORD=$TEST_PASSWORD
        if [ $? -ne 0 ]; then
            exit 1
        fi
        generate_report smoke
        validate_availability "k6-reports/smoke-results.json"
        if [ $? -ne 0 ]; then
            exit 1
        fi
        
        echo ""
        echo "[2/3] Load Tests..."
        k6 run --summary-export=k6-reports/load-results.json k6-load-test-login.js \
            -e BASE_URL=$BASE_URL \
            -e TEST_USERNAME=$TEST_USERNAME \
            -e TEST_PASSWORD=$TEST_PASSWORD
        if [ $? -ne 0 ]; then
            exit 1
        fi
        generate_report load
        validate_availability "k6-reports/load-results.json"
        if [ $? -ne 0 ]; then
            exit 1
        fi
        
        echo ""
        echo "[3/3] Stress Tests..."
        k6 run --summary-export=k6-reports/stress-results.json k6-stress-test-login.js \
            -e BASE_URL=$BASE_URL \
            -e TEST_USERNAME=$TEST_USERNAME \
            -e TEST_PASSWORD=$TEST_PASSWORD
        generate_report stress
        
        echo ""
        echo "============================================"
        echo "Todas las pruebas completadas exitosamente!"
        echo "============================================"
        ;;
    *)
        echo "ERROR: Tipo de prueba no reconocido: $TEST_TYPE"
        echo "Use: smoke, load, stress o all"
        exit 1
        ;;
esac

echo ""
echo "Reportes disponibles en: k6-reports/"

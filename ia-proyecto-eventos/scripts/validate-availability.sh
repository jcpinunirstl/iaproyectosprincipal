#!/bin/bash

RESULTS_FILE="$1"
MIN_AVAILABILITY="${2:-99}"

if [ ! -f "$RESULTS_FILE" ]; then
    echo "❌ Archivo de resultados no encontrado: $RESULTS_FILE"
    exit 1
fi

echo "📊 Validando resultados de prueba..."
echo ""

TOTAL_REQUESTS=$(jq '.summary.metrics.http_reqs.values.count // 0' "$RESULTS_FILE")
FAILED_REQUESTS=$(jq '.summary.metrics.http_reqs.values.fails // 0' "$RESULTS_FILE")
SUCCESSFUL_REQUESTS=$(jq '.summary.metrics.http_reqs.values.passes // 0' "$RESULTS_FILE")

if [ "$TOTAL_REQUESTS" -eq 0 ]; then
    echo "❌ No hay datos de pruebas en los resultados"
    exit 1
fi

AVAILABILITY=$(echo "scale=2; ($SUCCESSFUL_REQUESTS / $TOTAL_REQUESTS) * 100" | bc)

echo "📈 Estadísticas:"
echo "  • Total de solicitudes: $TOTAL_REQUESTS"
echo "  • Solicitudes exitosas: $SUCCESSFUL_REQUESTS"
echo "  • Solicitudes fallidas: $FAILED_REQUESTS"
echo "  • Disponibilidad: ${AVAILABILITY}%"
echo ""

if (( $(echo "$AVAILABILITY >= $MIN_AVAILABILITY" | bc -l) )); then
    echo "✅ PRUEBA EXITOSA: Disponibilidad ${AVAILABILITY}% >= ${MIN_AVAILABILITY}%"
    exit 0
else
    echo "❌ PRUEBA FALLIDA: Disponibilidad ${AVAILABILITY}% < ${MIN_AVAILABILITY}%"
    exit 1
fi

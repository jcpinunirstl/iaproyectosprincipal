param(
    [string]$ResultsFile = "k6-results.json",
    [int]$MinAvailability = 99
)

if (-not (Test-Path $ResultsFile)) {
    Write-Host "❌ Archivo de resultados no encontrado: $ResultsFile" -ForegroundColor Red
    exit 1
}

Write-Host "📊 Validando resultados de prueba..." -ForegroundColor Cyan
Write-Host ""

$data = Get-Content $ResultsFile | ConvertFrom-Json

$totalRequests = $data.metrics.http_reqs.values.count ?? 0
$failedRequests = $data.metrics.http_reqs.values.fails ?? 0
$successfulRequests = $totalRequests - $failedRequests

if ($totalRequests -eq 0) {
    Write-Host "❌ No hay datos de pruebas en los resultados" -ForegroundColor Red
    exit 1
}

$availability = [math]::Round(($successfulRequests / $totalRequests) * 100, 2)

Write-Host "📈 Estadísticas:" -ForegroundColor Cyan
Write-Host "  • Total de solicitudes: $totalRequests"
Write-Host "  • Solicitudes exitosas: $successfulRequests"
Write-Host "  • Solicitudes fallidas: $failedRequests"
Write-Host "  • Disponibilidad: ${availability}%"
Write-Host ""

if ($availability -ge $MinAvailability) {
    Write-Host "✅ PRUEBA EXITOSA: Disponibilidad ${availability}% >= ${MinAvailability}%" -ForegroundColor Green
    exit 0
} else {
    Write-Host "❌ PRUEBA FALLIDA: Disponibilidad ${availability}% < ${MinAvailability}%" -ForegroundColor Red
    exit 1
}

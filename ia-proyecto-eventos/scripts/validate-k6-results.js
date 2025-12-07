import { textSummary } from 'https://jslib.k6.io/k6-summary/0.0.1/index.js';
import fs from 'fs';

export function handleSummary(data) {
  const timestamp = new Date().toISOString().split('T')[0];
  const reportDir = `k6-reports`;
  
  if (!fs.existsSync(reportDir)) {
    fs.mkdirSync(reportDir, { recursive: true });
  }

  const jsonReport = {
    timestamp: new Date().toISOString(),
    summary: data,
    metrics: extractMetrics(data),
    validation: validateThresholds(data),
  };

  fs.writeFileSync(
    `${reportDir}/results-${timestamp}.json`,
    JSON.stringify(jsonReport, null, 2)
  );

  fs.writeFileSync(
    `${reportDir}/results-${timestamp}.html`,
    generateHtmlReport(jsonReport)
  );

  console.log('\n\n✅ Reportes generados en:', reportDir);
  
  return {
    'stdout': textSummary(data, { indent: ' ', enableColors: true }),
    [`${reportDir}/results-${timestamp}.json`]: JSON.stringify(jsonReport, null, 2),
    [`${reportDir}/results-${timestamp}.html`]: generateHtmlReport(jsonReport),
  };
}

function extractMetrics(data) {
  const metrics = {};
  
  if (data.metrics) {
    for (const [name, metric] of Object.entries(data.metrics)) {
      if (metric.values) {
        metrics[name] = {
          value: metric.values.value || 0,
          count: metric.values.count || 0,
          rate: metric.values.rate || 0,
        };
      }
    }
  }

  return metrics;
}

function validateThresholds(data) {
  const validation = {
    passed: true,
    issues: [],
  };

  const checks = data.checks || {};
  const httpErrors = data.metrics?.http_reqs?.values?.fails || 0;
  const httpSuccess = data.metrics?.http_reqs?.values?.passes || 0;
  const totalRequests = (httpSuccess || 0) + (httpErrors || 0);
  const successRate = totalRequests > 0 ? (httpSuccess / totalRequests) * 100 : 0;

  if (successRate < 99) {
    validation.passed = false;
    validation.issues.push(`❌ Disponibilidad: ${successRate.toFixed(2)}% (requerido: >= 99%)`);
  } else {
    validation.issues.push(`✅ Disponibilidad: ${successRate.toFixed(2)}%`);
  }

  for (const [checkName, checkData] of Object.entries(checks)) {
    const checkPassed = checkData.passes > 0;
    const checkFailed = checkData.fails > 0;
    
    if (checkFailed > 0) {
      validation.passed = false;
      validation.issues.push(`❌ Check falló: ${checkName} (${checkFailed} fallos)`);
    } else if (checkPassed > 0) {
      validation.issues.push(`✅ Check pasó: ${checkName}`);
    }
  }

  return validation;
}

function generateHtmlReport(report) {
  const validation = report.validation;
  const statusColor = validation.passed ? '#28a745' : '#dc3545';
  const statusText = validation.passed ? 'EXITOSA ✅' : 'FALLIDA ❌';

  const issuesHtml = validation.issues
    .map(issue => `<li>${issue}</li>`)
    .join('');

  const metricsHtml = Object.entries(report.metrics)
    .map(([name, values]) => `
      <tr>
        <td>${name}</td>
        <td>${values.value}</td>
        <td>${values.count}</td>
        <td>${values.rate ? values.rate.toFixed(2) : '0'}</td>
      </tr>
    `)
    .join('');

  return `
<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1">
    <title>K6 Load Test Report</title>
    <style>
        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }
        body {
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;
            background: #f5f5f5;
            padding: 20px;
        }
        .container {
            max-width: 1200px;
            margin: 0 auto;
            background: white;
            border-radius: 8px;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
            overflow: hidden;
        }
        .header {
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            padding: 40px;
            text-align: center;
        }
        .header h1 {
            font-size: 32px;
            margin-bottom: 10px;
        }
        .status {
            background: ${statusColor};
            color: white;
            padding: 15px 30px;
            border-radius: 5px;
            display: inline-block;
            font-size: 18px;
            font-weight: bold;
            margin-top: 10px;
        }
        .content {
            padding: 40px;
        }
        .section {
            margin-bottom: 40px;
        }
        .section h2 {
            color: #333;
            font-size: 24px;
            margin-bottom: 20px;
            border-bottom: 2px solid #667eea;
            padding-bottom: 10px;
        }
        .timestamp {
            color: #666;
            font-size: 14px;
            margin-bottom: 20px;
        }
        ul {
            list-style: none;
        }
        li {
            padding: 8px 0;
            font-size: 16px;
            color: #333;
        }
        table {
            width: 100%;
            border-collapse: collapse;
            margin-top: 20px;
        }
        th, td {
            padding: 12px;
            text-align: left;
            border-bottom: 1px solid #ddd;
        }
        th {
            background: #f8f9fa;
            font-weight: 600;
            color: #333;
        }
        tr:hover {
            background: #f8f9fa;
        }
        .footer {
            background: #f8f9fa;
            padding: 20px 40px;
            text-align: center;
            color: #666;
            font-size: 12px;
        }
    </style>
</head>
<body>
    <div class="container">
        <div class="header">
            <h1>📊 K6 Load Test Report</h1>
            <div class="status">${statusText}</div>
        </div>
        
        <div class="content">
            <div class="timestamp">
                <strong>Fecha:</strong> ${report.timestamp}
            </div>
            
            <div class="section">
                <h2>✓ Validaciones</h2>
                <ul>
                    ${issuesHtml}
                </ul>
            </div>
            
            ${metricsHtml ? `
            <div class="section">
                <h2>📈 Métricas</h2>
                <table>
                    <thead>
                        <tr>
                            <th>Métrica</th>
                            <th>Valor</th>
                            <th>Conteo</th>
                            <th>Tasa</th>
                        </tr>
                    </thead>
                    <tbody>
                        ${metricsHtml}
                    </tbody>
                </table>
            </div>
            ` : ''}
        </div>
        
        <div class="footer">
            Reporte generado automáticamente por K6 Load Testing
        </div>
    </div>
</body>
</html>
  `;
}

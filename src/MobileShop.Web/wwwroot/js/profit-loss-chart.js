document.addEventListener('DOMContentLoaded', function () {
    var canvas = document.getElementById('profitLossChart');
    if (!canvas || typeof Chart === 'undefined') return;

    var labels = JSON.parse(canvas.dataset.labels || '[]');
    var tickLabels = JSON.parse(canvas.dataset.tickLabels || '[]');
    var values = JSON.parse(canvas.dataset.values || '[]');
    var currentIndex = Number.parseInt(canvas.dataset.currentIndex || '-1', 10);
    var numberFormat = new Intl.NumberFormat(undefined, { maximumFractionDigits: 0 });
    var currentGuides = {
        id: 'currentGuides',
        afterDatasetsDraw: function (chart) {
            if (!Number.isInteger(currentIndex) || currentIndex < 0 || currentIndex >= values.length) return;

            var point = chart.getDatasetMeta(0).data[currentIndex];
            if (!point) return;

            var area = chart.chartArea;
            var context = chart.ctx;
            context.save();
            context.strokeStyle = '#6ae0be';
            context.lineWidth = 1.5;
            context.setLineDash([5, 4]);
            context.beginPath();
            context.moveTo(point.x, area.top);
            context.lineTo(point.x, area.bottom);
            context.moveTo(area.left, point.y);
            context.lineTo(area.right, point.y);
            context.stroke();

            context.setLineDash([]);
            context.lineWidth = 2;
            context.beginPath();
            context.arc(point.x, point.y, 7, 0, Math.PI * 2);
            context.stroke();

            var valueLabel = numberFormat.format(values[currentIndex]);
            context.font = '600 11px system-ui, sans-serif';
            context.textAlign = 'right';
            context.textBaseline = 'bottom';
            context.fillStyle = '#6ae0be';
            context.fillText(valueLabel, area.left - 8, point.y - 4);
            context.restore();
        }
    };

    new Chart(canvas, {
        type: 'line',
        data: {
            labels: labels,
            datasets: [{
                label: 'Net profit / loss',
                data: values,
                borderColor: '#a8b7ff',
                backgroundColor: 'rgba(145, 163, 255, 0.12)',
                borderWidth: 3,
                pointRadius: function (context) {
                    return context.dataIndex === currentIndex ? 5 : 3.5;
                },
                pointHoverRadius: 7,
                pointBackgroundColor: '#dce3ff',
                pointBorderColor: '#7187ff',
                pointBorderWidth: 2,
                tension: 0.2,
                fill: 'origin'
            }]
        },
        plugins: [currentGuides],
        options: {
            responsive: true,
            maintainAspectRatio: false,
            animation: { duration: 500 },
            interaction: { mode: 'nearest', intersect: true },
            plugins: {
                legend: { display: false },
                tooltip: {
                    callbacks: {
                        label: function (context) {
                            return 'Net profit / loss: ' + numberFormat.format(context.parsed.y) + ' IRR';
                        }
                    }
                }
            },
            layout: { padding: { top: 18, right: 20, bottom: 12, left: 12 } },
            scales: {
                x: {
                    offset: false,
                    grid: { display: false },
                    ticks: {
                        autoSkip: false,
                        maxRotation: labels.length > 14 ? 45 : 0,
                        minRotation: 0,
                        color: '#bac5db',
                        font: { size: labels.length > 14 ? 9 : 11 },
                        callback: function (value, index) {
                            return tickLabels[index] || this.getLabelForValue(value);
                        }
                    },
                    title: {
                        display: true,
                        text: canvas.dataset.intervalLabel,
                        color: '#aebbd5',
                        font: { size: 11, weight: '600' },
                        padding: { top: 14 }
                    },
                    border: { color: 'rgba(194, 207, 245, 0.28)' }
                },
                y: {
                    beginAtZero: true,
                    grid: { color: 'rgba(194, 207, 245, 0.12)' },
                    ticks: {
                        color: '#929fb8',
                        callback: function (value) { return numberFormat.format(value); }
                    },
                    title: {
                        display: true,
                        text: 'Net profit / loss (IRR)',
                        color: '#aebbd5',
                        font: { size: 11, weight: '600' }
                    },
                    border: { color: 'rgba(194, 207, 245, 0.28)' }
                }
            }
        }
    });
});

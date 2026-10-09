document.addEventListener('DOMContentLoaded', function () {
    var canvas = document.getElementById('profitLossChart');
    if (!canvas) return;
    if (typeof Chart === 'undefined') {
        console.error('Chart.js failed to load; the profit/loss chart cannot be rendered.');
        return;
    }

    function parseJsonDataset(name, fallback) {
        var raw = canvas.dataset[name];
        if (!raw) return fallback;

        try {
            return JSON.parse(raw);
        } catch (error) {
            console.error('Failed to parse dataset [' + name + ']:', error);
            return fallback;
        }
    }

    function formatToman(value) {
        return new Intl.NumberFormat('en-US', { maximumFractionDigits: 0 }).format(value).replace(/,/g, ' ') + ' Toman';
    }

    var labels = parseJsonDataset('labels', []);
    var tickLabels = parseJsonDataset('tickLabels', []);
    var values = parseJsonDataset('values', []);
    console.log('Chart data:', { labels, tickLabels, values });
    var currentIndex = Number.parseInt(canvas.dataset.currentIndex || '-1', 10);
    var numberFormat = new Intl.NumberFormat('en-US', { maximumFractionDigits: 0 });
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

            var valueLabel = formatToman(values[currentIndex]);
            context.font = '600 11px system-ui, sans-serif';
            context.textAlign = 'left';
            context.textBaseline = 'bottom';
            context.fillStyle = '#6ae0be';
            // Position label above the intersection point, offset to the right
            var labelX = point.x + 12;
            var labelY = point.y - 8;
            // Ensure label stays within chart bounds
            if (labelX + context.measureText(valueLabel).width > area.right - 4) {
                labelX = point.x - context.measureText(valueLabel).width - 12;
                context.textAlign = 'right';
            }
            context.fillText(valueLabel, labelX, labelY);
            context.restore();
        }
    };

    try {
        window.profitLossChart = new Chart(canvas, {
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
                                return 'Net profit / loss: ' + formatToman(context.parsed.y);
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
                        beginAtZero: false,
                        grid: { color: 'rgba(194, 207, 245, 0.12)' },
                        ticks: {
                            color: '#929fb8',
                            callback: function (value) { return numberFormat.format(value).replace(/,/g, ' '); }
                        },
                        title: {
                            display: true,
                            text: 'Net profit / loss (Toman)',
                            color: '#aebbd5',
                            font: { size: 11, weight: '600' }
                        },
                        border: { color: 'rgba(194, 207, 245, 0.28)' }
                    }
                }
            }
        });
    } catch (error) {
        console.error('Failed to render the profit/loss chart:', error);
    }
});

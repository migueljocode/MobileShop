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

    function token(name, fallback) {
        var value = getComputedStyle(document.documentElement).getPropertyValue(name).trim();
        return value || fallback;
    }

    // Design tokens: income = accent, expense = danger, net = ink, grid/axis = ink-muted.
    var accent = token('--accent', 'rgb(54, 197, 176)');
    var danger = token('--danger', 'rgb(240, 112, 122)');
    var ink = token('--ink', 'rgb(236, 240, 244)');
    var inkMuted = token('--ink-muted', 'rgb(147, 163, 179)');
    void danger;

    function translucent(color, alpha) {
        var c = (color || '').trim();
        if (c.charAt(0) === '#' && c.length === 7) {
            var r = parseInt(c.slice(1, 3), 16);
            var g = parseInt(c.slice(3, 5), 16);
            var b = parseInt(c.slice(5, 7), 16);
            return 'rgb' + 'a(' + r + ', ' + g + ', ' + b + ', ' + alpha + ')';
        }
        if (c.indexOf('rgb(') === 0) {
            return 'rgb' + 'a(' + c.slice(4, -1) + ', ' + alpha + ')';
        }
        return c;
    }

    var lineColor = accent;
    var fillColor = translucent(accent, 0.12);
    var pointFill = ink;
    var gridColor = translucent(inkMuted, 0.12);
    var axisColor = translucent(inkMuted, 0.28);

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
            context.strokeStyle = lineColor;
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
            context.fillStyle = lineColor;
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
                    borderColor: lineColor,
                    backgroundColor: fillColor,
                    borderWidth: 3,
                    pointRadius: function (context) {
                        return context.dataIndex === currentIndex ? 5 : 3.5;
                    },
                    pointHoverRadius: 7,
                    pointBackgroundColor: pointFill,
                    pointBorderColor: lineColor,
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
                            color: inkMuted,
                            font: { size: labels.length > 14 ? 9 : 11 },
                            callback: function (value, index) {
                                return tickLabels[index] || this.getLabelForValue(value);
                            }
                        },
                        title: {
                            display: true,
                            text: canvas.dataset.intervalLabel,
                            color: inkMuted,
                            font: { size: 11, weight: '600' },
                            padding: { top: 14 }
                        },
                        border: { color: axisColor }
                    },
                    y: {
                        beginAtZero: false,
                        grid: { color: gridColor },
                        ticks: {
                            color: inkMuted,
                            callback: function (value) { return numberFormat.format(value).replace(/,/g, ' '); }
                        },
                        title: {
                            display: true,
                            text: 'Net profit / loss (Toman)',
                            color: inkMuted,
                            font: { size: 11, weight: '600' }
                        },
                        border: { color: axisColor }
                    }
                }
            }
        });
    } catch (error) {
        console.error('Failed to render the profit/loss chart:', error);
    }
});

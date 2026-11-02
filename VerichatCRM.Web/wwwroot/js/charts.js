// Chart styling for the dashboards, in one place.
//
// Each view used to paste its own colour list next to its own Chart.js config.
// Dashboard/Index drew its first series as #8050ff and Analytics/Index reused it —
// a purple that is in no Verichat palette, in the single largest visual on the
// screen, while the button underneath it was #2F6FED. Here the palette is read
// from the stylesheet's custom properties, so a chart line is the same blue as the
// components around it and follows the brand the moment the tokens change.
(function (w) {
    'use strict';

    var css = getComputedStyle(document.documentElement);
    function token(name, fallback) {
        var v = css.getPropertyValue(name);
        return (v && v.trim()) || fallback;
    }

    var colors = {
        accent: token('--vc-accent', '#2F6FED'),
        cyan: token('--vc-cyan', '#00B4D8'),
        success: token('--vc-success', '#1FA971'),
        warning: token('--vc-warning', '#E8940C'),
        danger: token('--vc-danger', '#E24C4C'),
        neutral: token('--vc-neutral', '#4A5568')
    };
    var palette = [colors.accent, colors.cyan, colors.success, colors.warning, colors.danger, colors.neutral];
    var grid = token('--vc-border', '#E3E7EF');
    var ink = token('--vc-text', '#182233');
    var muted = token('--vc-text-muted', '#6B7385');
    var heading = token('--vc-font-heading', 'Poppins, sans-serif');

    // A series colour arrives as a hex token; an area fill needs it translucent.
    function tint(color, alpha) {
        var raw = (color || '').trim().replace('#', '');
        if (/^[0-9a-f]{3}$/i.test(raw)) raw = raw.split('').map(function (c) { return c + c; }).join('');
        if (!/^[0-9a-f]{6}$/i.test(raw)) return color;
        var n = parseInt(raw, 16);
        return 'rgba(' + ((n >> 16) & 255) + ',' + ((n >> 8) & 255) + ',' + (n & 255) + ',' + alpha + ')';
    }

    // Rand values in the band these charts actually plot, so the axis stays
    // readable in a 260px box: R 240k, R 1.4m.
    function rands(v) {
        var abs = Math.abs(v);
        if (abs >= 1e6) return 'R' + (v / 1e6).toFixed(1) + 'm';
        if (abs >= 1e3) return 'R' + Math.round(v / 1e3) + 'k';
        return 'R' + Math.round(v);
    }

    function canvas(id) {
        if (!w.Chart) return null;               // CDN unavailable: draw nothing, break nothing
        return document.getElementById(id);
    }

    var axis = {
        ticks: { color: muted, font: { family: heading, size: 11 } },
        grid: { color: grid },
        border: { color: grid }
    };

    var charts = {
        palette: palette,
        colors: colors,

        // A series may carry a color, plus fill (area under the line) or dashed
        // (a reference line such as a target). Procurement's revenue chart needed
        // both, and dashboard.js was the last view hand-writing a Chart.js config
        // to get them.
        line: function (id, labels, series, opts) {
            var el = canvas(id);
            if (!el) return null;
            opts = opts || {};
            var yTicks = { color: muted, font: { family: heading, size: 11 }, precision: 0 };
            // Assigned only when asked for: an explicit undefined callback would
            // shadow Chart.js's own default formatter for every other line chart.
            if (opts.money) yTicks.callback = rands;
            return new w.Chart(el, {
                type: 'line',
                data: {
                    labels: labels,
                    datasets: series.map(function (s, i) {
                        var color = s.color || palette[i % palette.length];
                        return {
                            label: s.label,
                            data: s.data,
                            borderColor: color,
                            backgroundColor: s.fill ? tint(color, 0.12) : 'transparent',
                            fill: !!s.fill,
                            borderDash: s.dashed ? [5, 5] : [],
                            tension: 0.35,
                            pointRadius: s.dashed ? 0 : (series.length > 1 ? 2 : 3),
                            pointBackgroundColor: color
                        };
                    })
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    plugins: {
                        legend: { display: series.length > 1, position: 'top', align: 'end',
                                  labels: { color: ink, font: { family: heading, size: 12 }, boxWidth: 12 } }
                    },
                    scales: { x: axis, y: { beginAtZero: true, ticks: yTicks, grid: axis.grid, border: axis.border } }
                }
            });
        },

        doughnut: function (id, labels, values, opts) {
            var el = canvas(id);
            if (!el) return null;
            opts = opts || {};
            var tooltip = {};
            // Same guard as the line chart's money axis: assigning an undefined
            // callback replaces Chart.js's own label formatter.
            if (opts.money) {
                tooltip.callbacks = {
                    label: function (c) {
                        var total = c.dataset.data.reduce(function (a, b) { return a + b; }, 0);
                        return c.label + ': ' + rands(c.parsed) + (total ? ' (' + Math.round(c.parsed / total * 100) + '%)' : '');
                    }
                };
            }
            return new w.Chart(el, {
                type: 'doughnut',
                data: {
                    labels: labels,
                    datasets: [{
                        data: values,
                        backgroundColor: labels.map(function (_, i) { return palette[i % palette.length]; }),
                        borderWidth: 0
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    plugins: {
                        legend: { position: 'right',
                                  labels: { color: ink, font: { family: heading, size: 12 }, boxWidth: 12, padding: 12 } },
                        tooltip: tooltip
                    }
                }
            });
        },

        bar: function (id, labels, series, opts) {
            var el = canvas(id);
            if (!el) return null;
            opts = opts || {};
            var yTicks = { color: muted, font: { family: heading, size: 11 }, precision: 0 };
            if (opts.money) yTicks.callback = rands;
            return new w.Chart(el, {
                type: 'bar',
                data: {
                    labels: labels,
                    datasets: series.map(function (s, i) {
                        return {
                            label: s.label,
                            data: s.data,
                            backgroundColor: s.color || palette[i % palette.length],
                            borderRadius: 4,
                            maxBarThickness: 28
                        };
                    })
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    plugins: {
                        legend: { display: series.length > 1, position: 'top', align: 'end',
                                  labels: { color: ink, font: { family: heading, size: 12 }, boxWidth: 12 } },
                        tooltip: opts.money
                            ? { callbacks: { label: function (c) { return c.dataset.label + ': ' + rands(c.parsed.y); } } }
                            : {}
                    },
                    scales: { x: axis, y: { beginAtZero: true, ticks: yTicks, grid: axis.grid, border: axis.border } }
                }
            });
        },

        // A pipeline funnel. Chart.js has no funnel type, so the trick — one
        // horizontal bar per stage, plotted as [-half, +half] so each centres on
        // zero and the stack visibly tapers — lives here instead of in the one
        // view that needed it. Analytics/Index used to carry it inline.
        funnel: function (id, stages) {
            var el = canvas(id);
            if (!el) return null;
            return new w.Chart(el, {
                type: 'bar',
                data: {
                    labels: stages.map(function (s) { return s.label + ' (' + s.value + ')'; }),
                    datasets: [{
                        data: stages.map(function (s) { return [-s.value / 2, s.value / 2]; }),
                        backgroundColor: palette,
                        borderRadius: 4,
                        barPercentage: 0.9,
                        categoryPercentage: 0.9
                    }]
                },
                options: {
                    indexAxis: 'y',
                    responsive: true,
                    maintainAspectRatio: false,
                    scales: {
                        x: { display: false },
                        y: { grid: { display: false }, border: { display: false },
                             ticks: { color: ink, font: { family: heading, size: 12 } } }
                    },
                    plugins: {
                        legend: { display: false },
                        tooltip: {
                            callbacks: {
                                label: function (c) {
                                    var value = stages[c.dataIndex].value;
                                    var prev = c.dataIndex > 0 ? stages[c.dataIndex - 1].value : 0;
                                    return prev ? value + ' (' + Math.round(value / prev * 100) + '% of previous stage)' : String(value);
                                }
                            }
                        }
                    }
                }
            });
        }
    };

    /**
     * The three charts of _DashboardCharts.cshtml, drawn from the JSON the partial
     * publishes. Each view used to call line/doughnut/line itself with its own
     * serialised arrays, which is how two dashboards ended up with two chart configs
     * of the same three panels.
     */
    function drawDashboard() {
        var node = document.getElementById('vc-dashboard-charts');
        if (!node) return;
        var d;
        try { d = JSON.parse(node.textContent); } catch (err) { return; }
        charts.line('revenueTrend', d.labels, [d.revenue]);
        charts.doughnut('statusBreakdown', d.status.labels, d.status.values);
        charts.line('seriesChart', d.labels, d.series);
    }

    charts.drawDashboard = drawDashboard;
    w.vcCharts = charts;
    // The payload is above this script in the document, so it is already parsed.
    drawDashboard();
})(window);

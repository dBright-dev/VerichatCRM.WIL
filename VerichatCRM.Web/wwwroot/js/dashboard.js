document.addEventListener('DOMContentLoaded', () => {
    const canvas = document.getElementById('revenueChart');
    if (!canvas || !window.revenueData) return;

    const labels = window.revenueData.map(d => d.Month);
    const revenue = window.revenueData.map(d => d.Revenue);
    const target = window.revenueData.map(d => d.Target);

    new Chart(canvas, {
        type: 'line',
        data: {
            labels,
            datasets: [
                {
                    label: 'Revenue',
                    data: revenue,
                    borderColor: '#00B4D8',
                    backgroundColor: 'rgba(0, 180, 216, 0.1)',
                    tension: 0.4,
                    fill: true,
                    pointBackgroundColor: '#00B4D8',
                    pointRadius: 4
                },
                {
                    label: 'Target',
                    data: target,
                    borderColor: '#E2E8F0',
                    borderDash: [5, 5],
                    tension: 0.4,
                    pointRadius: 0
                }
            ]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: { display: true, position: 'top', align: 'end' }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    ticks: { callback: v => 'R ' + (v / 1000).toFixed(0) + 'k' },
                    grid: { color: '#F0F4F8' }
                },
                x: { grid: { display: false } }
            }
        }
    });
});
document.addEventListener('DOMContentLoaded', () => {
    // Reordering is a server-side form POST (ProcurementController.Reorder): the
    // JWT is only in the session now, so page script cannot call the API itself.

    // Stock level chart
    const canvas = document.getElementById('stockLevelChart');
    if (!canvas || !window.stockLevels) return;

    new Chart(canvas, {
        type: 'bar',
        data: {
            labels: window.stockLevels.map(d => d.Month),
            datasets: [
                {
                    label: 'Desktop',
                    data: window.stockLevels.map(d => d.Desktop),
                    backgroundColor: '#00B4D8',
                    borderRadius: 6
                },
                {
                    label: 'Mobile',
                    data: window.stockLevels.map(d => d.Mobile),
                    backgroundColor: '#B794F4',
                    borderRadius: 6
                }
            ]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: { legend: { position: 'top' } },
            scales: {
                y: { beginAtZero: true, grid: { color: '#F0F4F8' } },
                x: { grid: { display: false } }
            }
        }
    });
});
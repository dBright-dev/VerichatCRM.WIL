document.addEventListener('DOMContentLoaded', () => {
    // Reorder buttons
    document.querySelectorAll('[data-sku]').forEach(btn => {
        btn.addEventListener('click', async () => {
            if (!confirm(`Create a reorder for ${btn.dataset.sku}?`)) return;
            try {
                await api.post('/api/purchaseorder', {
                    supplierName: 'Auto-Reorder',
                    lineItems: [{ sku: btn.dataset.sku, quantity: 1, unitCost: 0 }]
                });
                alert('Reorder created successfully.');
            } catch (err) {
                alert('Failed to create reorder: ' + err.message);
            }
        });
    });

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
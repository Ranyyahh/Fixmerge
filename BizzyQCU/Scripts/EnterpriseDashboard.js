const DashboardDataService = {
    getStats() {
        return Promise.resolve({
            totalSalesToday: 12500,
            ordersPending: 8,
            deliveriesActive: 3,
            newOrdersCount: 3
        });
    },
    getChartData(period) {
        const data = {
            daily: {
                labels: ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'],
                values: [2500, 4000, 5500, 8000, 12500, 10000],
                total: 'This Week: ₱ 69,467'
            },
            monthly: {
                labels: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun'],
                values: [45000, 52000, 48000, 60000, 75000, 69000],
                total: 'This Month: ₱ 349,000'
            }
        };
        return Promise.resolve(data[period]);
    }
};

let currentChart = null;

function renderStats(stats) {
    document.querySelector('.stat-total-sales').innerText = `₱ ${stats.totalSalesToday.toLocaleString()}`;
    document.querySelector('.stat-orders-pending').innerText = stats.ordersPending;
    document.querySelector('.stat-deliveries-active').innerText = stats.deliveriesActive;
    document.getElementById('newOrdersCount').innerText = stats.newOrdersCount;
}

function renderChart(period, chartData) {
    if (currentChart) currentChart.destroy();
    const canvas = document.getElementById('salesChart');
    canvas.style.height = '250px';
    currentChart = new Chart(canvas, {
        type: 'line',
        data: {
            labels: chartData.labels,
            datasets: [{
                label: 'Sales',
                data: chartData.values,
                borderWidth: 3,
                tension: 0.4,
                pointRadius: 5,
                fill: false
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: true,
            plugins: { legend: { display: false } },
            scales: { y: { beginAtZero: true, ticks: { callback: val => `₱${val}` } } }
        }
    });
    document.getElementById('weeklyTotal').innerText = chartData.total;
}

document.addEventListener('DOMContentLoaded', async function () {
    const stats = await DashboardDataService.getStats();
    renderStats(stats);

    let currentPeriod = 'daily';
    let chartData = await DashboardDataService.getChartData(currentPeriod);
    renderChart(currentPeriod, chartData);

    const periodSelect = document.getElementById('salesPeriodSelect');
    periodSelect.addEventListener('change', async (e) => {
        currentPeriod = e.target.value;
        const newData = await DashboardDataService.getChartData(currentPeriod);
        renderChart(currentPeriod, newData);
    });

    document.getElementById('addProductBtn').addEventListener('click', () => alert('Add Product - future'));
    document.getElementById('manageOrdersBtn').addEventListener('click', () => alert('Manage Orders - future'));
    document.getElementById('editStoreBtn').addEventListener('click', () => alert('Edit Store - future'));
    document.getElementById('newOrdersBtn').addEventListener('click', () => alert('Viewing new orders...'));

    const cards = document.querySelectorAll('.stats-card');
    cards.forEach(card => {
        card.addEventListener('mouseenter', () => card.style.transform = 'scale(1.02)');
        card.addEventListener('mouseleave', () => card.style.transform = 'scale(1)');
    });
});
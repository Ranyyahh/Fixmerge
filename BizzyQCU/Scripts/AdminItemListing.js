const params = new URLSearchParams(window.location.search);
const id = params.get('enterpriseId');

// TODO (DB): GET enterprise ID from URL

const CHART_DATA = {
    sales: {
        labels: ['Day 1', 'Day 2', 'Day 3', 'Day 4', 'Day 5'],
        values: [8, 19, 20, 33, 37],
        color: '#4A6CF7',
        title: 'Daily Sales'
    },
    ratings: {
        labels: ['Day 1', 'Day 2', 'Day 3', 'Day 4', 'Day 5'],
        values: [3.2, 3.8, 4.0, 4.3, 4.6],
        color: '#d4a017',
        title: 'Daily Ratings'
    }
};

function drawChart(canvas, dataset) {
    canvas.style.width = '';
    canvas.style.height = '';

    const wrap = canvas.parentElement;
    const W = wrap.clientWidth || 600;
    const H = wrap.clientHeight || 220;

    canvas.width = W;
    canvas.height = H;

    const ctx = canvas.getContext('2d');

    const pad = { top: 20, right: 20, bottom: 56, left: 44 };
    const chartW = W - pad.left - pad.right;
    const chartH = H - pad.top - pad.bottom;

    const vals = dataset.values;
    const labels = dataset.labels;
    const n = vals.length;

    const minVal = 0;
    const maxVal = Math.ceil(Math.max(...vals) * 1.2);
    const color = dataset.color;

    ctx.clearRect(0, 0, W, H);

    const xPos = i => pad.left + (i / (n - 1)) * chartW;
    const yPos = v => pad.top + chartH - ((v - minVal) / (maxVal - minVal)) * chartH;

    ctx.font = '11px DM Sans, sans-serif';
    ctx.fillStyle = '#9aa3bf';
    ctx.textAlign = 'right';

    const ySteps = 5;

    for (let i = 0; i <= ySteps; i++) {
        const v = minVal + (i / ySteps) * (maxVal - minVal);
        const y = yPos(v);

        ctx.strokeStyle = 'rgba(0,0,0,0.06)';
        ctx.beginPath();
        ctx.moveTo(pad.left, y);
        ctx.lineTo(pad.left + chartW, y);
        ctx.stroke();

        ctx.fillText(Math.round(v), pad.left - 6, y + 4);
    }

    ctx.textAlign = 'center';
    ctx.textBaseline = 'top';

    labels.forEach((lbl, i) => {
        ctx.fillStyle = '#9aa3bf';
        ctx.fillText(lbl, xPos(i), pad.top + chartH + 8);
    });

    ctx.beginPath();
    ctx.moveTo(xPos(0), yPos(vals[0]));

    for (let i = 1; i < n; i++) {
        const x0 = xPos(i - 1);
        const y0 = yPos(vals[i - 1]);
        const x1 = xPos(i);
        const y1 = yPos(vals[i]);
        const cpx = (x0 + x1) / 2;

        ctx.bezierCurveTo(cpx, y0, cpx, y1, x1, y1);
    }

    ctx.lineTo(xPos(n - 1), pad.top + chartH);
    ctx.lineTo(xPos(0), pad.top + chartH);
    ctx.closePath();

    const grad = ctx.createLinearGradient(0, pad.top, 0, pad.top + chartH);
    grad.addColorStop(0, color + '30');
    grad.addColorStop(1, color + '05');

    ctx.fillStyle = grad;
    ctx.fill();

    ctx.beginPath();
    ctx.moveTo(xPos(0), yPos(vals[0]));

    for (let i = 1; i < n; i++) {
        const x0 = xPos(i - 1);
        const y0 = yPos(vals[i - 1]);
        const x1 = xPos(i);
        const y1 = yPos(vals[i]);
        const cpx = (x0 + x1) / 2;

        ctx.bezierCurveTo(cpx, y0, cpx, y1, x1, y1);
    }

    ctx.strokeStyle = color;
    ctx.lineWidth = 2.5;
    ctx.stroke();

    vals.forEach((v, i) => {
        ctx.beginPath();
        ctx.arc(xPos(i), yPos(v), 4, 0, Math.PI * 2);
        ctx.fillStyle = color;
        ctx.fill();
        ctx.strokeStyle = '#fff';
        ctx.lineWidth = 2;
        ctx.stroke();
    });
}

let activeTab = 'sales';

window.addEventListener('DOMContentLoaded', () => {
    const canvas = document.getElementById('detailChart');

    if (canvas) {
        renderChart('sales');

        window.addEventListener('resize', () => {
            renderChart(activeTab);
        });
    }

    const tabSales = document.getElementById('tabSales');
    const tabRatings = document.getElementById('tabRatings');

    if (tabSales && tabRatings) {
        tabSales.addEventListener('click', () => {
            if (activeTab === 'sales') return;

            activeTab = 'sales';
            tabSales.classList.add('active');
            tabRatings.classList.remove('active');

            renderChart('sales');
        });

        tabRatings.addEventListener('click', () => {
            if (activeTab === 'ratings') return;

            activeTab = 'ratings';
            tabRatings.classList.add('active');
            tabSales.classList.remove('active');

            renderChart('ratings');
        });
    }

    const modalBackdrop = document.getElementById('deleteModal');
    const deleteBtn = document.querySelector('.delete-btn');

    function closeModal() {
        if (modalBackdrop) {
            modalBackdrop.classList.remove('active');
        }
    }

    if (deleteBtn && modalBackdrop) {
        deleteBtn.addEventListener('click', () => {
            modalBackdrop.classList.add('active');
        });
    }

    const cancelBtn = document.querySelector('.modal-cancel');

    if (cancelBtn) {
        cancelBtn.addEventListener('click', closeModal);
    }

    if (modalBackdrop) {
        modalBackdrop.addEventListener('click', e => {
            if (e.target === modalBackdrop) {
                closeModal();
            }
        });
    }

    document.addEventListener('keydown', e => {
        if (e.key === 'Escape') {
            closeModal();
        }
    });

    const searchInput = document.getElementById('searchInput');

    if (searchInput) {
        searchInput.addEventListener('input', function () {
            const query = this.value.toLowerCase().trim();
            const products = document.querySelectorAll('.product-card');

            products.forEach(card => {
                const name = card.dataset.name.toLowerCase();

                if (query === '') {
                    card.style.display = '';
                } else if (name.includes(query)) {
                    card.style.display = '';
                } else {
                    card.style.display = 'none';
                }
            });
        });
    }

    const productSearch = document.getElementById('productSearch');

    if (productSearch) {
        productSearch.addEventListener('input', function () {
            const query = this.value.toLowerCase().trim();
            const products = document.querySelectorAll('.product-card');

            products.forEach(card => {
                const name = card.dataset.name.toLowerCase();

                if (query === '') {
                    card.style.display = '';
                } else if (name.includes(query)) {
                    card.style.display = '';
                } else {
                    card.style.display = 'none';
                }
            });
        });
    }
});

function renderChart(tab) {
    const canvas = document.getElementById('detailChart');
    const title = document.getElementById('chartTitle');

    if (!canvas || !title) return;

    const dataset = CHART_DATA[tab];
    title.textContent = dataset.title;

    drawChart(canvas, dataset);
}

function openModal() {
    const modal = document.getElementById('deleteModal');

    if (modal) {
        modal.classList.add('active');
    }
}

function closeModal() {
    const modal = document.getElementById('deleteModal');

    if (modal) {
        modal.classList.remove('active');
    }
}

function confirmDelete() {
    closeModal();
    alert('Account deleted');
}

function approveItem() {
    alert('Item approved');
}

function removeItem() {
    alert('Item removed');
}
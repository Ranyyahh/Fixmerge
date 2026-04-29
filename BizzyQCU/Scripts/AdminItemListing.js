// ============================================================
// AdminItemListing.js (FULL FIXED VERSION)
// ============================================================

// ── TODO (DB): GET enterprise ID from URL ──
const params = new URLSearchParams(window.location.search);
const id = params.get('enterpriseId'); // FIXED (was commented only)

// ── EXAMPLE DATA (REMOVE WHEN DB IS READY) ──
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

// ── CHART DRAW FUNCTION ──
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

    // GRID + Y AXIS
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

    // LABELS
    ctx.textAlign = 'center';
    ctx.textBaseline = 'top';

    labels.forEach((lbl, i) => {
        ctx.fillStyle = '#9aa3bf';
        ctx.fillText(lbl, xPos(i), pad.top + chartH + 8);
    });

    // AREA
    ctx.beginPath();
    ctx.moveTo(xPos(0), yPos(vals[0]));

    for (let i = 1; i < n; i++) {
        const x0 = xPos(i - 1), y0 = yPos(vals[i - 1]);
        const x1 = xPos(i), y1 = yPos(vals[i]);
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

    // LINE
    ctx.beginPath();
    ctx.moveTo(xPos(0), yPos(vals[0]));

    for (let i = 1; i < n; i++) {
        const x0 = xPos(i - 1), y0 = yPos(vals[i - 1]);
        const x1 = xPos(i), y1 = yPos(vals[i]);
        const cpx = (x0 + x1) / 2;
        ctx.bezierCurveTo(cpx, y0, cpx, y1, x1, y1);
    }

    ctx.strokeStyle = color;
    ctx.lineWidth = 2.5;
    ctx.stroke();

    // POINTS
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

// ── STATE ──
let activeTab = 'sales';

// ── SAFE INIT ──
window.addEventListener('DOMContentLoaded', () => {

    const canvas = document.getElementById('detailChart');
    if (!canvas) return;

    renderChart('sales');

    window.addEventListener('resize', () => {
        renderChart(activeTab);
    });

    // ── TABS ──
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

    // ── DELETE MODAL ──
    const modalBackdrop = document.getElementById('modalBackdrop');
    const deleteBtn = document.getElementById('deleteBtn');
    const cancelDelete = document.getElementById('cancelDelete');
    const confirmDelete = document.getElementById('confirmDelete');

    function closeModal() {
        modalBackdrop.classList.remove('active');
    }

    if (deleteBtn) {
        deleteBtn.addEventListener('click', () => {
            const name = document.getElementById('profileName').textContent;
            document.getElementById('modalEnterpriseName').textContent = name;
            modalBackdrop.classList.add('active');
        });
    }

    if (cancelDelete) cancelDelete.addEventListener('click', closeModal);

    if (modalBackdrop) {
        modalBackdrop.addEventListener('click', (e) => {
            if (e.target === modalBackdrop) closeModal();
        });
    }

    document.addEventListener('keydown', (e) => {
        if (e.key === 'Escape') closeModal();
    });

    if (confirmDelete) {
        confirmDelete.addEventListener('click', () => {

            confirmDelete.textContent = 'Deleting...';
            confirmDelete.disabled = true;

            setTimeout(() => {
                closeModal();
                confirmDelete.textContent = 'Yes, Delete';
                confirmDelete.disabled = false;

                window.location.href = '/AdminPanel/AdminLandingEntrep';

            }, 1200);
        });
    }

    // ── FIXED NAVIGATION (NO RAZOR IN JS) ──
    const viewListingBtn = document.getElementById('viewListingBtn');

    if (viewListingBtn) {
        viewListingBtn.addEventListener('click', () => {
            window.location.href = '/AdminPanel/AdminItemListing';
        });
    }

    const viewDocsBtn = document.getElementById('viewDocsBtn');

    if (viewDocsBtn) {
        viewDocsBtn.addEventListener('click', () => {
            alert('View Documents — connect backend');
        });
    }

    const removeItemBtn = document.getElementById('removeItemBtn');

    if (removeItemBtn) {
        removeItemBtn.addEventListener('click', () => {
            alert('Remove Item — connect backend');
        });
    }

    // ── PRODUCT SEARCH ──
    const productSearchBtn = document.getElementById('productSearchBtn');
    const productSearch = document.getElementById('productSearch');

    function searchProduct() {
        const query = productSearch.value.trim();
        if (!query) return;
        console.log('Product search:', query);
    }

    if (productSearchBtn) productSearchBtn.addEventListener('click', searchProduct);

    if (productSearch) {
        productSearch.addEventListener('keydown', (e) => {
            if (e.key === 'Enter') searchProduct();
        });
    }
});

// ── RENDER ──
function renderChart(tab) {
    const canvas = document.getElementById('detailChart');
    if (!canvas) return;

    const dataset = CHART_DATA[tab];

    document.getElementById('chartTitle').textContent = dataset.title;

    drawChart(canvas, dataset);
}

// ── HELPERS (DB READY) ──
function populateProfile(enterprise) {
    document.getElementById('profileName').textContent = enterprise.name;
    document.getElementById('profileId').textContent = enterprise.id;
    document.getElementById('profileEmail').textContent = enterprise.email;

    const statusEl = document.getElementById('profileStatus');
    statusEl.textContent = enterprise.status;
    statusEl.className = 'info-value status-' + enterprise.status.toLowerCase();

    const starsEl = document.getElementById('profileStars');
    starsEl.innerHTML = '';

    for (let i = 1; i <= 5; i++) {
        const s = document.createElement('span');
        s.className = 'star' + (i <= enterprise.rating ? ' filled' : '');
        s.textContent = '★';
        starsEl.appendChild(s);
    }

    document.getElementById('chartSubtitle').textContent = enterprise.name;
}

function populateProduct(product) {
    document.getElementById('productName').textContent = product.name;
    document.getElementById('productPrice').textContent = '₱' + Number(product.price).toFixed(2);

    if (product.imageUrl) {
        document.getElementById('productImg').src = product.imageUrl;
    }
}
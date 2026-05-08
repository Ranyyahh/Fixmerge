// ============================================================
// AdminEditEntrep.js - Dynamic Enterprise Details
// ============================================================

// Get enterprise ID from URL
const urlParams = new URLSearchParams(window.location.search);
const enterpriseId = urlParams.get('enterpriseId');

let currentProductId = null;
let activeTab = 'sales';
let allProducts = [];

// Load enterprise details on page load
document.addEventListener('DOMContentLoaded', function () {
    console.log('Enterprise ID from URL:', enterpriseId);

    if (enterpriseId && enterpriseId !== 'null' && enterpriseId !== 'undefined') {
        loadEnterpriseDetails();
        loadEnterpriseProducts();
        loadSalesData();
        loadRatingsData();
    } else {
        console.error('No valid enterprise ID provided in URL');
        document.getElementById('profileName').textContent = 'No Enterprise Selected';
        document.getElementById('profileId').textContent = 'Invalid ID';
    }
});

function loadEnterpriseDetails() {
    console.log('Fetching enterprise details for ID:', enterpriseId);

    fetch(`/AdminPanel/GetEnterpriseDetails?enterpriseId=${enterpriseId}`)
        .then(response => response.json())
        .then(data => {
            console.log('Enterprise details received:', data);
            if (data && data.EnterpriseId) {
                populateProfile(data);
                const chartSubtitle = document.getElementById('chartSubtitle');
                if (chartSubtitle) chartSubtitle.textContent = data.StoreName;
            } else if (data.message) {
                console.error('API message:', data.message);
                showError(data.message);
            } else {
                console.error('No enterprise data received');
                showError('Enterprise not found');
            }
        })
        .catch(error => {
            console.error('Error loading enterprise:', error);
            showError('Failed to load enterprise details: ' + error.message);
        });
}

function loadEnterpriseProducts() {
    console.log('Fetching products for enterprise ID:', enterpriseId);

    fetch(`/AdminPanel/GetEnterpriseProducts?enterpriseId=${enterpriseId}`)
        .then(response => response.json())
        .then(data => {
            console.log('Products received:', data);
            allProducts = data || [];
            if (allProducts.length > 0) {
                populateProduct(allProducts[0]);
            } else {
                console.log('No products found for this enterprise');
                document.getElementById('productName').textContent = 'No products available';
                document.getElementById('productPrice').textContent = '₱0.00';
            }
        })
        .catch(error => {
            console.error('Error loading products:', error);
            document.getElementById('productName').textContent = 'Error loading products';
        });
}

function loadSalesData() {
    fetch(`/AdminPanel/GetSalesData?enterpriseId=${enterpriseId}&days=7`)
        .then(response => response.json())
        .then(data => {
            console.log('Sales data:', data);
            updateChartData('sales', data);
        })
        .catch(error => console.error('Error loading sales data:', error));
}

function loadRatingsData() {
    fetch(`/AdminPanel/GetRatingsData?enterpriseId=${enterpriseId}&days=7`)
        .then(response => response.json())
        .then(data => {
            console.log('Ratings data:', data);
            updateChartData('ratings', data);
        })
        .catch(error => console.error('Error loading ratings data:', error));
}

function updateChartData(tab, data) {
    if (!data || data.length === 0) {
        // Use default data if no data
        const defaultData = {
            labels: ['No Data', 'No Data', 'No Data', 'No Data', 'No Data'],
            values: [0, 0, 0, 0, 0]
        };
        CHART_DATA[tab].labels = defaultData.labels;
        CHART_DATA[tab].values = defaultData.values;
    } else {
        CHART_DATA[tab].labels = data.map(d => d.Label);
        CHART_DATA[tab].values = data.map(d => parseFloat(d.Value));
    }

    if (activeTab === tab) {
        renderChart(tab);
    }
}

function populateProfile(enterprise) {
    console.log('Populating profile with:', enterprise);

    const nameEl = document.getElementById('profileName');
    if (nameEl) nameEl.textContent = enterprise.StoreName || 'Enterprise Name';

    const idEl = document.getElementById('profileId');
    if (idEl) idEl.textContent = enterprise.EnterpriseId || 'N/A';

    const emailEl = document.getElementById('profileEmail');
    if (emailEl) emailEl.textContent = enterprise.Email || 'No email';

    const statusEl = document.getElementById('profileStatus');
    const status = enterprise.Status || 'pending';
    if (statusEl) {
        statusEl.textContent = status.charAt(0).toUpperCase() + status.slice(1);
        statusEl.className = 'info-value status-' + status.toLowerCase();
    }

    const rating = enterprise.RatingAvg || 0;
    const starsEl = document.getElementById('profileStars');
    if (starsEl) {
        starsEl.innerHTML = '';
        const roundedRating = Math.round(rating);
        for (let i = 1; i <= 5; i++) {
            const starSpan = document.createElement('span');
            starSpan.className = 'star' + (i <= roundedRating ? ' filled' : '');
            starSpan.textContent = '★';
            starsEl.appendChild(starSpan);
        }
    }

    const firstLetter = enterprise.StoreName ? enterprise.StoreName.charAt(0).toUpperCase() : 'E';
    const colors = ['#4A6CF7', '#7B3FE4', '#d4a017', '#2e4dbf', '#e03131'];
    const colorIndex = (enterprise.EnterpriseId || 1) % colors.length;
    const bgColor = colors[colorIndex];

    const avatarDiv = document.getElementById('profileAvatar');
    if (avatarDiv) {
        avatarDiv.innerHTML = `<div style="width: 72px; height: 72px; border-radius: 50%; background: ${bgColor}; display: flex; align-items: center; justify-content: center;">
                                  <span style="color: white; font-size: 32px; font-weight: 600;">${escapeHtml(firstLetter)}</span>
                              </div>`;
    }
}

function populateProduct(product) {
    currentProductId = product.ProductId;
    const productNameEl = document.getElementById('productName');
    const productPriceEl = document.getElementById('productPrice');

    if (productNameEl) productNameEl.textContent = product.ProductName || 'No product';
    if (productPriceEl) productPriceEl.textContent = '₱' + (product.Price || 0).toFixed(2);
}

function showError(message) {
    const nameEl = document.getElementById('profileName');
    if (nameEl) nameEl.textContent = 'Error';
    const idEl = document.getElementById('profileId');
    if (idEl) idEl.textContent = message;
}

function escapeHtml(str) {
    if (!str) return '';
    return str
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;')
        .replace(/'/g, '&#39;');
}

// ============================================================
// CHART FUNCTIONS
// ============================================================

let CHART_DATA = {
    sales: {
        labels: ['Day 1', 'Day 2', 'Day 3', 'Day 4', 'Day 5'],
        values: [0, 0, 0, 0, 0],
        color: '#4A6CF7',
        title: 'Daily Sales (₱)'
    },
    ratings: {
        labels: ['Day 1', 'Day 2', 'Day 3', 'Day 4', 'Day 5'],
        values: [0, 0, 0, 0, 0],
        color: '#d4a017',
        title: 'Daily Ratings (★)'
    }
};

function drawChart(canvas, dataset) {
    if (!canvas) return;

    const wrap = canvas.parentElement;
    const W = wrap ? (wrap.clientWidth || 600) : 600;
    const H = wrap ? (wrap.clientHeight || 220) : 220;

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
    const maxVal = Math.max(...vals, 1) * 1.2;
    const color = dataset.color;

    ctx.clearRect(0, 0, W, H);

    if (n === 0 || vals.every(v => v === 0)) {
        ctx.fillStyle = '#9aa3bf';
        ctx.font = '14px DM Sans, sans-serif';
        ctx.textAlign = 'center';
        ctx.fillText('No data available', W / 2, H / 2);
        return;
    }

    function xPos(i) { return pad.left + (i / (n - 1)) * chartW; }
    function yPos(v) { return pad.top + chartH - ((v - minVal) / (maxVal - minVal)) * chartH; }

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
        ctx.fillText(v.toFixed(1), pad.left - 6, y + 4);
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

function renderChart(tab) {
    const canvas = document.getElementById('detailChart');
    if (!canvas) return;

    const dataset = CHART_DATA[tab];
    const titleEl = document.getElementById('chartTitle');
    if (titleEl) titleEl.textContent = dataset.title;
    drawChart(canvas, dataset);
}

// Delete modal functionality
const modalBackdrop = document.getElementById('modalBackdrop');
const deleteBtn = document.getElementById('deleteBtn');
const cancelDelete = document.getElementById('cancelDelete');
const confirmDelete = document.getElementById('confirmDelete');

if (deleteBtn) {
    deleteBtn.addEventListener('click', () => {
        const name = document.getElementById('profileName').textContent;
        const modalName = document.getElementById('modalEnterpriseName');
        if (modalName) modalName.textContent = name;
        if (modalBackdrop) modalBackdrop.classList.add('active');
    });
}

if (cancelDelete) {
    cancelDelete.addEventListener('click', () => {
        if (modalBackdrop) modalBackdrop.classList.remove('active');
    });
}

if (modalBackdrop) {
    modalBackdrop.addEventListener('click', (e) => {
        if (e.target === modalBackdrop) {
            modalBackdrop.classList.remove('active');
        }
    });
}

document.addEventListener('keydown', (e) => {
    if (e.key === 'Escape' && modalBackdrop && modalBackdrop.classList.contains('active')) {
        modalBackdrop.classList.remove('active');
    }
});

if (confirmDelete) {
    confirmDelete.addEventListener('click', () => {
        confirmDelete.textContent = 'Deleting…';
        confirmDelete.disabled = true;

        fetch('/AdminPanel/DeleteEnterprise', {
            method: 'POST',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
            body: `enterpriseId=${enterpriseId}`
        })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    alert('Enterprise deleted successfully');
                    window.location.href = '/AdminPanel/AdminLandingEntrep';
                } else {
                    alert('Failed to delete enterprise: ' + (data.message || 'Unknown error'));
                    confirmDelete.textContent = 'Yes, Delete';
                    confirmDelete.disabled = false;
                    if (modalBackdrop) modalBackdrop.classList.remove('active');
                }
            })
            .catch(error => {
                console.error('Error:', error);
                alert('An error occurred');
                confirmDelete.textContent = 'Yes, Delete';
                confirmDelete.disabled = false;
                if (modalBackdrop) modalBackdrop.classList.remove('active');
            });
    });
}

// Search product functionality
function searchProduct() {
    const query = document.getElementById('productSearch').value.trim();
    if (!query) return;

    const found = allProducts.find(p => p.ProductName && p.ProductName.toLowerCase().includes(query.toLowerCase()));
    if (found) {
        populateProduct(found);
    } else {
        alert('Product not found');
    }
}

// Product search button
const productSearchBtn = document.getElementById('productSearchBtn');
const productSearchInput = document.getElementById('productSearch');

if (productSearchBtn) {
    productSearchBtn.addEventListener('click', searchProduct);
}
if (productSearchInput) {
    productSearchInput.addEventListener('keydown', (e) => {
        if (e.key === 'Enter') searchProduct();
    });
}

// View listing button redirect
// View listing button redirect - FIXED
const viewListingBtn = document.getElementById('viewListingBtn');
if (viewListingBtn) {
    viewListingBtn.addEventListener('click', () => {
        window.location.href = `/AdminPanel/AdminItemListing?enterpriseId=${enterpriseId}`;
    });
}

// Tab switching for chart
const tabSalesEl = document.getElementById('tabSales');
const tabRatingsEl = document.getElementById('tabRatings');

if (tabSalesEl && tabRatingsEl) {
    tabSalesEl.addEventListener('click', () => {
        if (activeTab === 'sales') return;
        activeTab = 'sales';
        tabSalesEl.classList.add('active');
        tabRatingsEl.classList.remove('active');
        renderChart('sales');
    });

    tabRatingsEl.addEventListener('click', () => {
        if (activeTab === 'ratings') return;
        activeTab = 'ratings';
        tabRatingsEl.classList.add('active');
        tabSalesEl.classList.remove('active');
        renderChart('ratings');
    });
}
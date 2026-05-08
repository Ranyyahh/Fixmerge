// ============================================================
// AdminItemListing.js - Dynamic Products Listing
// ============================================================

// Get enterprise ID from URL - declare once only
const urlParamsItem = new URLSearchParams(window.location.search);
const enterpriseIdItem = urlParamsItem.get('enterpriseId');
let allProductsItem = [];
let selectedProductIdItem = null;

document.addEventListener('DOMContentLoaded', function () {
    console.log('Enterprise ID from URL:', enterpriseIdItem);

    if (enterpriseIdItem && enterpriseIdItem !== 'null' && enterpriseIdItem !== 'undefined') {
        loadEnterpriseDetailsItem();
        loadProductsItem();
    } else {
        console.error('No enterprise ID provided');
        document.getElementById('profileName').textContent = 'No Enterprise Selected';
        document.getElementById('productsGrid').innerHTML = '<div class="empty-state">No enterprise selected</div>';
    }

    setupEventListenersItem();
});

function loadEnterpriseDetailsItem() {
    console.log('Fetching enterprise details for ID:', enterpriseIdItem);

    fetch(`/AdminPanel/GetEnterpriseDetails?enterpriseId=${enterpriseIdItem}`)
        .then(response => {
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            return response.json();
        })
        .then(data => {
            console.log('Enterprise details received:', data);
            if (data && data.EnterpriseId) {
                populateProfileItem(data);
            } else if (data.message) {
                console.error('API message:', data.message);
                document.getElementById('profileName').textContent = 'Enterprise not found';
            }
        })
        .catch(error => {
            console.error('Error loading enterprise:', error);
            document.getElementById('profileName').textContent = 'Error loading enterprise';
        });
}

function loadProductsItem() {
    console.log('Fetching products for enterprise ID:', enterpriseIdItem);

    fetch(`/AdminPanel/GetProductsForListing?enterpriseId=${enterpriseIdItem}`)
        .then(response => response.json())
        .then(data => {
            console.log('Products received:', data);
            allProductsItem = data || [];
            renderProductsItem(allProductsItem);
        })
        .catch(error => {
            console.error('Error loading products:', error);
            document.getElementById('productsGrid').innerHTML = '<div class="empty-state">Error loading products</div>';
        });
}

function populateProfileItem(enterprise) {
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
            starSpan.textContent = '?';
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
                                  <span style="color: white; font-size: 32px; font-weight: 600;">${escapeHtmlItem(firstLetter)}</span>
                              </div>`;
    }
}

function renderProductsItem(products) {
    const grid = document.getElementById('productsGrid');
    if (!grid) return;

    if (!products || products.length === 0) {
        grid.innerHTML = '<div class="empty-state">No products found for this enterprise</div>';
        return;
    }

    grid.innerHTML = '';
    products.forEach(product => {
        const card = document.createElement('div');
        card.className = 'product-card';
        if (product.Status === 'pending') {
            card.classList.add('pending');
        }
        card.dataset.id = product.ProductId;
        card.dataset.name = (product.ProductName || '').toLowerCase();
        card.addEventListener('click', () => selectProductItem(product.ProductId));

        const statusBadge = product.Status === 'pending'
            ? '<span class="status-badge pending">Pending Approval</span>'
            : '<span class="status-badge active">Active</span>';

        card.innerHTML = `
            <div class="product-image-wrap">
                <div class="product-placeholder">${escapeHtmlItem((product.ProductName || 'P').charAt(0))}</div>
            </div>
            <div class="product-info-wrap">
                <div class="product-title">${escapeHtmlItem(product.ProductName || 'Unknown')}</div>
                <div class="product-detail-line">
                    <span class="detail-label">Price:</span> ?${parseFloat(product.Price || 0).toFixed(2)}
                </div>
                <div class="product-detail-line">
                    <span class="detail-label">Description:</span> ${escapeHtmlItem(product.Description || 'No description')}
                </div>
                ${statusBadge}
            </div>
        `;
        grid.appendChild(card);
    });
}

function selectProductItem(productId) {
    selectedProductIdItem = productId;
    const cards = document.querySelectorAll('.product-card');
    cards.forEach(card => {
        if (parseInt(card.dataset.id) === productId) {
            card.classList.add('selected');
        } else {
            card.classList.remove('selected');
        }
    });
}

function setupEventListenersItem() {
    const productSearch = document.getElementById('productSearch');
    if (productSearch) {
        productSearch.addEventListener('input', function () {
            const query = this.value.toLowerCase().trim();
            const filtered = allProductsItem.filter(p => (p.ProductName || '').toLowerCase().includes(query));
            renderProductsItem(filtered);
        });
    }

    const approveBtn = document.getElementById('approveItemBtn');
    if (approveBtn) {
        approveBtn.addEventListener('click', () => {
            if (selectedProductIdItem) {
                approveProductItem(selectedProductIdItem);
            } else {
                showToastItem('Please select a product first', 'error');
            }
        });
    }

    const removeBtn = document.getElementById('removeItemBtn');
    if (removeBtn) {
        removeBtn.addEventListener('click', () => {
            if (selectedProductIdItem) {
                if (confirm('Are you sure you want to remove this product?')) {
                    removeProductItem(selectedProductIdItem);
                }
            } else {
                showToastItem('Please select a product first', 'error');
            }
        });
    }

    // Delete enterprise modal
    const deleteBtn = document.getElementById('deleteBtn');
    const modalBackdrop = document.getElementById('deleteModal');
    const cancelDelete = document.getElementById('cancelDelete');
    const confirmDeleteBtn = document.getElementById('confirmDelete');

    if (deleteBtn && modalBackdrop) {
        deleteBtn.addEventListener('click', () => {
            const name = document.getElementById('profileName').textContent;
            const modalName = document.getElementById('modalEnterpriseName');
            if (modalName) modalName.textContent = name;
            modalBackdrop.classList.add('active');
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

    if (confirmDeleteBtn) {
        confirmDeleteBtn.addEventListener('click', () => {
            fetch('/AdminPanel/DeleteEnterprise', {
                method: 'POST',
                headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
                body: `enterpriseId=${enterpriseIdItem}`
            })
                .then(response => response.json())
                .then(data => {
                    if (data.success) {
                        showToastItem('Enterprise deleted successfully', 'success');
                        setTimeout(() => {
                            window.location.href = '/AdminPanel/AdminLandingEntrep';
                        }, 1500);
                    } else {
                        showToastItem('Failed to delete enterprise: ' + (data.message || 'Unknown error'), 'error');
                        if (modalBackdrop) modalBackdrop.classList.remove('active');
                    }
                })
                .catch(error => {
                    console.error('Error:', error);
                    showToastItem('An error occurred', 'error');
                    if (modalBackdrop) modalBackdrop.classList.remove('active');
                });
        });
    }
}

function approveProductItem(productId) {
    const formData = new URLSearchParams();
    formData.append('productId', productId);

    fetch('/AdminPanel/ApproveProduct', {
        method: 'POST',
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        body: formData
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                showToastItem('Product approved successfully', 'success');
                loadProductsItem();
                selectedProductIdItem = null;
            } else {
                showToastItem('Failed to approve product: ' + (data.message || 'Unknown error'), 'error');
            }
        })
        .catch(error => {
            console.error('Error:', error);
            showToastItem('An error occurred', 'error');
        });
}

function removeProductItem(productId) {
    const formData = new URLSearchParams();
    formData.append('productId', productId);

    fetch('/AdminPanel/RemoveProduct', {
        method: 'POST',
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        body: formData
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                showToastItem('Product removed successfully', 'success');
                loadProductsItem();
                selectedProductIdItem = null;
            } else {
                showToastItem('Failed to remove product: ' + (data.message || 'Unknown error'), 'error');
            }
        })
        .catch(error => {
            console.error('Error:', error);
            showToastItem('An error occurred', 'error');
        });
}

function showToastItem(message, type) {
    const toast = document.getElementById('toast');
    if (!toast) return;

    toast.textContent = message;
    toast.className = 'toast ' + type;
    toast.style.display = 'block';

    setTimeout(() => {
        toast.style.display = 'none';
    }, 3000);
}

function escapeHtmlItem(str) {
    if (!str) return '';
    return str
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;')
        .replace(/'/g, '&#39;');
}
/*View reports button toh man, hahaha paganahin nalang pag natapos na tayo */
const viewReportsBtn = document.getElementById('viewReportsBtn');
if (viewReportsBtn) {
    viewReportsBtn.addEventListener('click', () => {
        alert('Reports feature coming soon');
    });
}

/*Pre nag n-null sa akin yung docs pati yung qcu id pre  */
const viewDocsBtn = document.getElementById('viewDocsBtn');
if (viewDocsBtn) {
    viewDocsBtn.addEventListener('click', () => {
        alert('Documents feature coming soon');
    });
}
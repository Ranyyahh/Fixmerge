// ManageOrders.js - Updated Working Version

document.addEventListener('DOMContentLoaded', function () {
    loadPendingOrders();
});

async function loadPendingOrders() {
    const container = document.getElementById('ordersContainer');
    if (!container) return;

    container.innerHTML = '<div class="loading-orders">Loading orders...</div>';

    try {
        const response = await fetch('/ManageOrders/GetPendingOrders');
        const data = await response.json();

        if (data.success && data.orders && data.orders.length > 0) {
            renderOrderCards(data.orders);
        } else {
            container.innerHTML = `
                <div class="empty-orders">
                    <p>🎉 No pending orders!</p>
                    <p class="empty-subtitle">All orders have been processed.</p>
                </div>
            `;
        }
    } catch (error) {
        console.error('Error loading orders:', error);
        container.innerHTML = `
            <div class="error-orders">
                <p>❌ Failed to load orders</p>
                <p class="empty-subtitle">Please try again later.</p>
            </div>
        `;
    }
}

function renderOrderCards(orders) {
    const container = document.getElementById('ordersContainer');
    if (!container) return;

    let cardsHtml = '';

    orders.forEach(order => {
        // Support both camelCase and PascalCase property names
        const orderId = order.orderId || order.OrderId || order.id;
        const customerName = order.customerName || order.CustomerName || 'Unknown Customer';
        const totalAmount = order.totalAmount || order.TotalAmount || 0;
        const orderTime = order.orderTime || order.OrderTime || '';
        const orderDateFormatted = order.orderDateFormatted || order.OrderDateFormatted || '';
        const deliveryOption = order.deliveryOption || order.DeliveryOption || 'pickup';
        const orderStatus = order.status || order.Status || 'preparing';

        const statusClass = orderStatus === 'preparing' ? 'status-preparing' : 'status-pending';
        const statusText = orderStatus === 'preparing' ? 'Preparing' : 'Pending';

        cardsHtml += `
            <div class="glass-card" data-order-id="${orderId}" onclick="viewOrder(${orderId})">
                <div class="card-header">
                    <div>
                        <h4 class="cust-name">${escapeHtml(customerName)}</h4>
                        <span class="card-timestamp">${orderDateFormatted} • ${orderTime}</span>
                    </div>
                    <span class="badge-pill ${statusClass}" id="pill-${orderId}">${statusText}</span>
                </div>
                <div class="order-summary">
                    <p class="item-count">${deliveryOption === 'pickup' ? '📍 Pickup' : '🚚 Delivery'}</p>
                    <p class="item-list">Total: ₱ ${Number(totalAmount).toFixed(2)}</p>
                </div>
                <div class="card-footer">
                    <span class="price-text">₱ ${Number(totalAmount).toFixed(2)}</span>
                    <div id="action-area-${orderId}">
                        <button class="btn-status" onclick="toggleDropdown(event, ${orderId})">Change Status ▾</button>
                    </div>
                </div>
                <div class="status-dropdown" id="dropdown-${orderId}" style="display:none">
                    <button onclick="updateStatus(event, ${orderId}, 'completed')">✅ Complete Order</button>
                    <button onclick="updateStatus(event, ${orderId}, 'cancelled')">❌ Cancel Order</button>
                </div>
            </div>
        `;
    });

    container.innerHTML = cardsHtml;
}

async function updateStatus(event, orderId, newStatus) {
    if (event) event.stopPropagation();

    if (!orderId) {
        showNotification('Invalid order ID', 'error');
        return;
    }

    try {
        const response = await fetch('/ManageOrders/UpdateOrderStatus', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ orderId: orderId, status: newStatus })
        });

        const data = await response.json();

        if (data.success) {
            showNotification(data.message || 'Status updated!', 'success');
            setTimeout(() => {
                loadPendingOrders();
            }, 1000);
        } else {
            showNotification(data.message || 'Failed to update status', 'error');
        }
    } catch (error) {
        console.error('Error updating status:', error);
        showNotification('Failed to update order status', 'error');
    }

    const dropdown = document.getElementById(`dropdown-${orderId}`);
    if (dropdown) dropdown.style.display = 'none';
}

function toggleDropdown(event, orderId) {
    event.stopPropagation();

    document.querySelectorAll('.status-dropdown').forEach(d => {
        d.style.display = 'none';
    });

    const dropdown = document.getElementById(`dropdown-${orderId}`);
    if (dropdown) {
        dropdown.style.display = dropdown.style.display === 'flex' ? 'none' : 'flex';
    }
}

async function viewOrder(orderId) {
    if (!orderId) {
        showNotification('Invalid order ID', 'error');
        return;
    }

    try {
        const response = await fetch(`/ManageOrders/GetOrderDetails?orderId=${orderId}`);

        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        const data = await response.json();

        if (data.success && data.order) {
            const order = data.order;

            let itemsHtml = '';
            if (order.items && order.items.length > 0) {
                itemsHtml = '<div class="items-list">';
                order.items.forEach(item => {
                    itemsHtml += `
                        <div class="item-row">
                            <span class="item-qty">${item.quantity}x</span>
                            <span class="item-name">${escapeHtml(item.productName)}</span>
                            <span class="item-price">₱ ${Number(item.unitPrice).toFixed(2)}</span>
                        </div>
                    `;
                });
                itemsHtml += '</div>';
            } else {
                itemsHtml = '<div class="items-list">No items found</div>';
            }

            const content = `
                <div class="details-container">
                    <h1 class="order-id-header">Order #QCU-${order.orderId}</h1>
                    <p class="order-subtitle">${escapeHtml(order.customerName)} • ${order.orderTime || ''}</p>

                    <div class="detail-group">
                        <div class="detail-row">
                            <span class="detail-label">📍 Location</span>
                            <span class="detail-value">${order.deliveryOption === 'pickup' ? 'Store Pickup' : escapeHtml(order.customerLocation || 'Campus Delivery')}</span>
                        </div>
                        <div class="detail-row">
                            <span class="detail-label">💳 Payment</span>
                            <span class="detail-value">${order.paymentMethod ? order.paymentMethod.toUpperCase() : 'Cash'}</span>
                        </div>
                        <div class="detail-row">
                            <span class="detail-label">📦 Status</span>
                            <span class="detail-value status-${order.status}">${order.status ? order.status.toUpperCase() : 'PENDING'}</span>
                        </div>
                        <div class="detail-row">
                            <span class="detail-label">🛒 Items</span>
                        </div>
                        ${itemsHtml}
                        ${order.deliveryFee > 0 ? `
                        <div class="detail-row">
                            <span class="detail-label">🚚 Delivery Fee</span>
                            <span class="detail-value">₱ ${Number(order.deliveryFee).toFixed(2)}</span>
                        </div>
                        ` : ''}
                    </div>

                    ${order.orderNote ? `
                        <div class="note-box">
                            <p class="note-text">"${escapeHtml(order.orderNote)}"</p>
                        </div>
                    ` : ''}

                    <div class="total-section">
                        <span class="total-label">Total Amount</span>
                        <span class="total-amount">₱ ${Number(order.totalAmount).toFixed(2)}</span>
                    </div>

                    <p class="courier-label">👤 CUSTOMER DETAILS</p>
                    <div class="courier-card">
                        <div class="courier-info">
                            <div class="courier-avatar">${escapeHtml(order.customerName).charAt(0)}</div>
                            <div>
                                <p class="courier-name">${escapeHtml(order.customerName)}</p>
                                <p class="courier-type">${order.customerPhone || 'No phone number'}</p>
                            </div>
                        </div>
                        ${order.customerPhone ? `<button class="btn-message-emoji" onclick="window.location.href='tel:${order.customerPhone}'">📞 Call</button>` : ''}
                    </div>
                </div>
            `;

            if (window.innerWidth > 1100) {
                const detailsPanel = document.getElementById('detailsPanel');
                if (detailsPanel) {
                    detailsPanel.innerHTML = content;
                }
            } else {
                const modalContent = document.getElementById('modal-content');
                if (modalContent) {
                    modalContent.innerHTML = content;
                }
                const mobileModal = document.getElementById('mobile-modal');
                if (mobileModal) {
                    mobileModal.style.display = 'flex';
                }
            }
        } else {
            showNotification('Failed to load order details', 'error');
        }
    } catch (error) {
        console.error('Error loading order details:', error);
        showNotification('Failed to load order details', 'error');
    }
}

function closeDetails() {
    const detailsPanel = document.getElementById('detailsPanel');
    if (detailsPanel && window.innerWidth > 1100) {
        detailsPanel.innerHTML = `
            <div class="sidebar-placeholder">
                <p class="sidebar-placeholder-text">Select an order card<br>to view details</p>
            </div>
        `;
    }
    const mobileModal = document.getElementById('mobile-modal');
    if (mobileModal) {
        mobileModal.style.display = 'none';
    }
}

function showNotification(message, type) {
    const notification = document.createElement('div');
    notification.className = `notification ${type}`;
    notification.innerText = message;
    notification.style.cssText = `
        position: fixed;
        bottom: 20px;
        right: 20px;
        padding: 12px 20px;
        border-radius: 8px;
        color: white;
        z-index: 9999;
        background-color: ${type === 'success' ? '#4CAF50' : '#f44336'};
        animation: slideIn 0.3s ease;
    `;
    document.body.appendChild(notification);
    setTimeout(() => notification.remove(), 3000);
}

function escapeHtml(text) {
    if (!text) return '';
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

window.onclick = function (event) {
    if (!event.target.closest('.btn-status')) {
        document.querySelectorAll('.status-dropdown').forEach(d => d.style.display = 'none');
    }
}
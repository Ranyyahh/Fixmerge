const LOCAL_IMAGE_BASE = '/Images/';   

const productImageMap = {
    'Pasta Verde': '1.png',
    'Pasta Verde Filipino Style Spag': '2.png'
};
function getImageUrl(filename) { return LOCAL_IMAGE_BASE + filename; }

let cartItems = [];

function loadInitialCart() {
    cartItems = [
        { Id: 1, EnterpriseName: "QCU Enterprise", ProductName: "Pasta Verde", UnitPrice: 67, Quantity: 1, ImageUrl: getImageUrl(productImageMap['Pasta Verde']) },
        { Id: 2, EnterpriseName: "QCU Enterprise", ProductName: "Pasta Verde Filipino Style Spag", UnitPrice: 67, Quantity: 3, ImageUrl: getImageUrl(productImageMap['Pasta Verde Filipino Style Spag']) }
    ];
    renderCart();
}

function getCurrentEnterprise() {
    if (cartItems.length === 0) return null;
    return cartItems[0].EnterpriseName;
}
function canAddProduct(enterpriseName) {
    if (cartItems.length === 0) return true;
    return getCurrentEnterprise() === enterpriseName;
}
function addProductToCart(productName, unitPrice, enterpriseName, imageFilename) {
    if (!canAddProduct(enterpriseName)) {
        alert(`⚠️ Orders are limited to "${getCurrentEnterprise()}" only. Please complete or clear current order before ordering from another enterprise.`);
        return false;
    }
    const newId = Date.now() + Math.floor(Math.random() * 10000);
    const imageUrl = imageFilename ? getImageUrl(imageFilename) : getImageUrl('default.jpg');
    cartItems.push({
        Id: newId,
        EnterpriseName: enterpriseName,
        ProductName: productName,
        UnitPrice: unitPrice,
        Quantity: 1,
        ImageUrl: imageUrl
    });
    renderCart();
    return true;
}
function updateQuantity(itemId, newQty) {
    if (newQty < 1) newQty = 1;
    const item = cartItems.find(i => i.Id === itemId);
    if (item) { item.Quantity = newQty; renderCart(); }
}
function removeItem(itemId) {
    cartItems = cartItems.filter(i => i.Id !== itemId);
    renderCart();
}
function calculateTotal() {
    return cartItems.reduce((sum, item) => sum + (item.UnitPrice * item.Quantity), 0);
}

function escapeHtml(str) {
    if (!str) return '';
    return str.replace(/[&<>]/g, m => m === '&' ? '&amp;' : m === '<' ? '&lt;' : '&gt;');
}

function renderCart() {
    const tbody = document.getElementById('cartBody');
    if (!tbody) return;
    tbody.innerHTML = '';
    const container = document.getElementById('enterpriseHeaderContainer');

    if (!cartItems.length) {
        tbody.innerHTML = `<tr><td colspan="6" class="text-center text-muted">🛒 No products ordered. You can add products from the catalog.</td></tr>`;
        document.getElementById('totalPayment').innerText = '₱0';
        if (container) container.style.display = 'none';
        return;
    }

    const groups = new Map();
    cartItems.forEach(i => { if (!groups.has(i.EnterpriseName)) groups.set(i.EnterpriseName, []); groups.get(i.EnterpriseName).push(i); });

    if (container) {
        container.innerHTML = `<div class="enterprise-header"><i class="fas fa-store"></i> ${escapeHtml(cartItems[0].EnterpriseName)}</div>`;
        container.style.display = 'block';
    }

    for (let [ent, items] of groups.entries()) {
        items.forEach(item => {
            const subtotal = item.UnitPrice * item.Quantity;
            const row = document.createElement('tr');
            row.classList.add('product-row');
            row.innerHTML = `
                <td data-label="Image" style="text-align:center"><img src="${escapeHtml(item.ImageUrl)}" class="product-image" onerror="this.src='/Images/default.jpg'"></td>
                <td data-label="Product" class="product-name">${escapeHtml(item.ProductName)}</td>
                <td data-label="Unit Price">₱${item.UnitPrice.toFixed(2)}</td>
                <td data-label="Quantity"><input type="number" class="quantity-input" data-id="${item.Id}" value="${item.Quantity}" min="1" step="1"></td>
                <td data-label="Subtotal">₱${subtotal.toFixed(2)}</td>
                <td data-label="Action" class="text-center"><i class="fas fa-trash-alt action-icon" data-id="${item.Id}"></i></td>
            `;
            tbody.appendChild(row);
        });
    }

    document.getElementById('totalPayment').innerHTML = `₱${calculateTotal().toFixed(2)}`;

    document.querySelectorAll('.quantity-input').forEach(input => {
        input.removeEventListener('change', handleQuantityChange);
        input.addEventListener('change', handleQuantityChange);
    });
    document.querySelectorAll('.action-icon').forEach(icon => {
        icon.removeEventListener('click', handleRemove);
        icon.addEventListener('click', handleRemove);
    });
}

function handleQuantityChange(e) {
    const id = parseInt(e.target.getAttribute('data-id'));
    const newVal = parseInt(e.target.value);
    if (!isNaN(newVal)) updateQuantity(id, newVal);
}
function handleRemove(e) {
    const id = parseInt(e.target.getAttribute('data-id'));
    if (confirm('Remove item?')) removeItem(id);
}
function buyMore() { alert("This will redirect to the product catalog page in the future."); }

function initDeliveryPayment() {
    updateRoomVisibility();
    document.querySelectorAll('.change-btn').forEach(btn => {
        btn.removeEventListener('click', changeHandler);
        btn.addEventListener('click', changeHandler);
    });
}
function changeHandler(e) {
    const type = e.target.getAttribute('data-type');
    if (type === 'delivery') {
        const span = document.getElementById('deliveryOptionDisplay');
        span.innerText = span.innerText === 'Room to Room' ? 'Pickup' : 'Room to Room';
        updateRoomVisibility();
    } else if (type === 'payment') {
        const span = document.getElementById('paymentMethodDisplay');
        span.innerText = span.innerText === 'Cash on Delivery' ? 'Gcash' : 'Cash on Delivery';
    }
}
function updateRoomVisibility() {
    const isRoom = document.getElementById('deliveryOptionDisplay').innerText === 'Room to Room';
    const container = document.getElementById('roomSpecifyContainer');
    container.style.display = isRoom ? 'block' : 'none';
    if (!isRoom) document.getElementById('roomSpecifyInput').value = '';
}
async function placeOrder() {
    if (!cartItems.length) { alert("Your cart is empty."); return; }
    const total = calculateTotal();
    const delivery = document.getElementById('deliveryOptionDisplay').innerText;
    let roomInfo = '';
    if (delivery === 'Room to Room') {
        const room = document.getElementById('roomSpecifyInput').value.trim();
        roomInfo = room ? `\nRoom: ${room}` : '\nRoom: (not specified)';
    }
    const payment = document.getElementById('paymentMethodDisplay').innerText;
    const note = document.getElementById('orderNote').value;
    alert(`✅ Order placed successfully!\n\nTotal: ₱${total.toFixed(2)}\nDelivery: ${delivery}${roomInfo}\nPayment: ${payment}\nNote: ${note || '(none)'}\n\nYou may now order from another enterprise.`);
    cartItems = [];
    renderCart();
    document.getElementById('orderNote').value = '';
    document.getElementById('roomSpecifyInput').value = '';
}

document.addEventListener('DOMContentLoaded', () => {
    loadInitialCart();
    initDeliveryPayment();
    document.getElementById('buyMoreBtn').addEventListener('click', buyMore);
    document.getElementById('placeOrderBtn').addEventListener('click', placeOrder);
});

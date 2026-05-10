// ─── Cart State ────────────────────────────────────────────────────────────────
let cartItems = [];

// ─── Load cart from sessionStorage (set by ProductChild page) ─────────────────
function loadCartFromSession() {
    var stored = sessionStorage.getItem('bizzyCart');
    if (stored) {
        try { cartItems = JSON.parse(stored); } catch (e) { cartItems = []; }
    }
    renderCart();
}

function saveCartToSession() {
    sessionStorage.setItem('bizzyCart', JSON.stringify(cartItems));
}

// ─── Called by ProductChild page when "Order Now" is clicked ──────────────────
// Usage in Pchild.js:
//   CartBridge.addItem(productId, productName, unitPrice, enterpriseId, enterpriseName, imageBase64);
window.CartBridge = {
    addItem: function (productId, productName, unitPrice, enterpriseId, enterpriseName, imageBase64) {
        var stored = sessionStorage.getItem('bizzyCart');
        var cart = [];
        if (stored) { try { cart = JSON.parse(stored); } catch (e) { cart = []; } }

        // Enforce single-enterprise rule
        if (cart.length > 0 && cart[0].EnterpriseId !== enterpriseId) {
            alert('⚠️ You can only order from one enterprise at a time. Complete or clear your current order first.');
            return false;
        }

        // If product already in cart, increase quantity
        var existing = null;
        for (var i = 0; i < cart.length; i++) {
            if (cart[i].ProductId === productId) { existing = cart[i]; break; }
        }

        if (existing) {
            existing.Quantity += 1;
        } else {
            cart.push({
                Id: Date.now(),
                ProductId: productId,
                ProductName: productName,
                UnitPrice: parseFloat(unitPrice),
                Quantity: 1,
                EnterpriseId: enterpriseId,
                EnterpriseName: enterpriseName,
                ImageBase64: imageBase64 || ''
            });
        }

        sessionStorage.setItem('bizzyCart', JSON.stringify(cart));
        return true;
    }
};

// ─── Cart Operations ───────────────────────────────────────────────────────────
function updateQuantity(itemId, newQty) {
    if (newQty < 1) newQty = 1;
    for (var i = 0; i < cartItems.length; i++) {
        if (cartItems[i].Id === itemId) { cartItems[i].Quantity = newQty; break; }
    }
    saveCartToSession();
    renderCart();
}

function removeItem(itemId) {
    cartItems = cartItems.filter(function (i) { return i.Id !== itemId; });
    saveCartToSession();
    renderCart();
}

function calculateTotal() {
    var sum = 0;
    cartItems.forEach(function (item) { sum += item.UnitPrice * item.Quantity; });
    return sum;
}

function getDeliveryFee() {
    var delivery = document.getElementById('deliveryOptionDisplay').innerText.trim();
    // Flat fee for room-to-room / campus delivery — adjust as needed
    if (delivery === 'Room to Room') return 20;
    if (delivery === 'Campus Delivery') return 50;
    return 0;
}

function escapeHtml(str) {
    if (!str) return '';
    return str.replace(/[&<>"']/g, function (m) {
        return { '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[m];
    });
}

// ─── Render ────────────────────────────────────────────────────────────────────
function renderCart() {
    var tbody = document.getElementById('cartBody');
    if (!tbody) return;
    tbody.innerHTML = '';
    var container = document.getElementById('enterpriseHeaderContainer');

    if (!cartItems.length) {
        tbody.innerHTML = '<tr><td colspan="6" class="text-center text-muted">🛒 No products ordered. Add products from the catalog.</td></tr>';
        document.getElementById('totalPayment').innerText = '₱0.00';
        if (container) container.style.display = 'none';
        return;
    }

    if (container) {
        container.innerHTML = '<div class="enterprise-header"><i class="fas fa-store"></i> ' + escapeHtml(cartItems[0].EnterpriseName) + '</div>';
        container.style.display = 'block';
    }

    cartItems.forEach(function (item) {
        var subtotal = item.UnitPrice * item.Quantity;
        var imgHtml;
        if (item.ImageBase64) {
            imgHtml = '<img src="data:image/jpeg;base64,' + item.ImageBase64 + '" class="product-image">';
        } else {
            imgHtml = '<img src="/Images/default.jpg" class="product-image">';
        }

        var row = document.createElement('tr');
        row.classList.add('product-row');
        row.innerHTML =
            '<td data-label="Image" style="text-align:center">' + imgHtml + '</td>' +
            '<td data-label="Product" class="product-name">' + escapeHtml(item.ProductName) + '</td>' +
            '<td data-label="Unit Price">&#8369;' + item.UnitPrice.toFixed(2) + '</td>' +
            '<td data-label="Quantity"><input type="number" class="quantity-input" data-id="' + item.Id + '" value="' + item.Quantity + '" min="1" step="1"></td>' +
            '<td data-label="Subtotal">&#8369;' + subtotal.toFixed(2) + '</td>' +
            '<td data-label="Action" class="text-center"><i class="fas fa-trash-alt action-icon" data-id="' + item.Id + '"></i></td>';
        tbody.appendChild(row);
    });

    var fee = getDeliveryFee();
    var grandTotal = calculateTotal() + fee;
    document.getElementById('totalPayment').innerHTML = '&#8369;' + grandTotal.toFixed(2);

    document.querySelectorAll('.quantity-input').forEach(function (input) {
        input.addEventListener('change', handleQuantityChange);
    });
    document.querySelectorAll('.action-icon').forEach(function (icon) {
        icon.addEventListener('click', handleRemove);
    });
}

function handleQuantityChange(e) {
    var id = parseInt(e.target.getAttribute('data-id'));
    var val = parseInt(e.target.value);
    if (!isNaN(val)) updateQuantity(id, val);
}
function handleRemove(e) {
    var id = parseInt(e.target.getAttribute('data-id'));
    if (confirm('Remove this item?')) removeItem(id);
}

// ─── Delivery / Payment UI ─────────────────────────────────────────────────────
function initDeliveryPayment() {
    updateRoomVisibility();
    document.querySelectorAll('.change-btn').forEach(function (btn) {
        btn.addEventListener('click', changeHandler);
    });
}

function changeHandler(e) {
    var type = e.target.getAttribute('data-type');
    if (type === 'delivery') {
        var span = document.getElementById('deliveryOptionDisplay');
        var options = ['Pickup', 'Room to Room', 'Campus Delivery'];
        var current = options.indexOf(span.innerText.trim());
        span.innerText = options[(current + 1) % options.length];
        updateRoomVisibility();
        renderCart(); // re-render to update total with new delivery fee
    } else if (type === 'payment') {
        var pspan = document.getElementById('paymentMethodDisplay');
        pspan.innerText = pspan.innerText.trim() === 'Cash on Delivery' ? 'GCash' : 'Cash on Delivery';
    }
}

function updateRoomVisibility() {
    var delivery = document.getElementById('deliveryOptionDisplay').innerText.trim();
    var container = document.getElementById('roomSpecifyContainer');
    var isRoom = (delivery === 'Room to Room' || delivery === 'Campus Delivery');
    container.style.display = isRoom ? 'block' : 'none';
    if (!isRoom) document.getElementById('roomSpecifyInput').value = '';
}

// ─── Place Order ───────────────────────────────────────────────────────────────
function placeOrder() {
    if (!cartItems.length) { alert('Your cart is empty.'); return; }

    var delivery = document.getElementById('deliveryOptionDisplay').innerText.trim();
    var payment = document.getElementById('paymentMethodDisplay').innerText.trim();
    var note = document.getElementById('orderNote').value.trim();
    var roomInput = document.getElementById('roomSpecifyInput').value.trim();
    var location = (delivery === 'Pickup') ? 'Store Pickup' : (roomInput || 'Not specified');
    var deliveryFee = getDeliveryFee();
    var grandTotal = calculateTotal() + deliveryFee;

    // Map display text → DB enum value
    var deliveryOption;
    if (delivery === 'Room to Room') { deliveryOption = 'room_to_room'; }
    else if (delivery === 'Campus Delivery') { deliveryOption = 'campus_delivery'; }
    else { deliveryOption = 'pickup'; }

    var paymentMethod = (payment === 'GCash') ? 'gcash' : 'cash';

    // Build items payload
    var itemsPayload = cartItems.map(function (item) {
        return { ProductId: item.ProductId, Quantity: item.Quantity, UnitPrice: item.UnitPrice };
    });

    // Get anti-forgery token
    var token = '';
    var tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
    if (tokenInput) token = tokenInput.value;

    var btn = document.getElementById('placeOrderBtn');
    btn.disabled = true;
    btn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Placing...';

    fetch('/Checkout/PlaceOrder', {
        method: 'POST',
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        body: [
            'enterpriseId=' + encodeURIComponent(cartItems[0].EnterpriseId),
            'totalAmount=' + encodeURIComponent(grandTotal.toFixed(2)),
            'paymentMethod=' + encodeURIComponent(paymentMethod),
            'deliveryOption=' + encodeURIComponent(deliveryOption),
            'customerLocation=' + encodeURIComponent(location),
            'orderNote=' + encodeURIComponent(note),
            'deliveryFee=' + encodeURIComponent(deliveryFee.toFixed(2)),
            'itemsJson=' + encodeURIComponent(JSON.stringify(itemsPayload)),
            '__RequestVerificationToken=' + encodeURIComponent(token)
        ].join('&')
    })
        .then(function (res) { return res.json(); })
        .then(function (data) {
            if (data.success) {
                sessionStorage.removeItem('bizzyCart');
                cartItems = [];
                window.location.href = '/Tracking/Tracking?orderId=' + data.orderId;
            } else {
                alert('❌ Order failed: ' + (data.message || 'Unknown error.'));
                btn.disabled = false;
                btn.innerHTML = '<i class="fas fa-check-circle"></i> Place Order';
            }
        })
        .catch(function (err) {
            alert('❌ Network error. Please try again.');
            btn.disabled = false;
            btn.innerHTML = '<i class="fas fa-check-circle"></i> Place Order';
            console.error(err);
        });
}

// ─── Init ──────────────────────────────────────────────────────────────────────
document.addEventListener('DOMContentLoaded', function () {
    loadCartFromSession();
    initDeliveryPayment();

    document.getElementById('buyMoreBtn').addEventListener('click', function () {
        window.location.href = '/ProductList/ProductList';
    });
    document.getElementById('placeOrderBtn').addEventListener('click', placeOrder);
});
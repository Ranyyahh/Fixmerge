document.addEventListener("DOMContentLoaded", function () {
    var searchInput = document.getElementById('productSearch');
    var categoryFilter = document.getElementById('categoryFilter');
    var popularityFilter = document.getElementById('popularityFilter');
    var priceFilter = document.getElementById('priceFilter');
    var noResults = document.getElementById('noResults');
    var filterToggleBtn = document.getElementById('filterToggleBtn');
    var filterSection = document.getElementById('filterSection');

    /* ---- Mobile filter panel toggle ---- */
    if (filterToggleBtn && filterSection) {
        filterToggleBtn.addEventListener('click', function () {
            var isOpen = filterSection.classList.toggle('filter-section--open');
            filterToggleBtn.classList.toggle('filter-icon-btn--active', isOpen);
        });
    }

    /* ---- Product filtering ---- */
    function filterProducts() {
        var searchVal = searchInput.value.toLowerCase();
        var categoryVal = categoryFilter.value;
        var popularityVal = popularityFilter.value;
        var priceVal = priceFilter.value;
        var cards = document.querySelectorAll('.product-card');
        var visible = 0;

        cards.forEach(function (card) {
            var name = card.querySelector('.product-name').textContent.toLowerCase();
            var store = card.querySelector('.product-store').textContent.toLowerCase();

            var matchSearch = name.includes(searchVal) || store.includes(searchVal);
            var matchCategory = !categoryVal || card.dataset.category === categoryVal;
            var matchPopularity = !popularityVal || card.dataset.popularity === popularityVal;
            var matchPrice = !priceVal || card.dataset.price === priceVal;

            if (matchSearch && matchCategory && matchPopularity && matchPrice) {
                card.style.display = '';
                visible++;
            } else {
                card.style.display = 'none';
            }
        });

        if (noResults) noResults.style.display = visible === 0 ? 'block' : 'none';
    }

    if (searchInput) searchInput.addEventListener('input', filterProducts);
    if (categoryFilter) categoryFilter.addEventListener('change', filterProducts);
    if (popularityFilter) popularityFilter.addEventListener('change', filterProducts);
    if (priceFilter) priceFilter.addEventListener('change', filterProducts);

    /* ========== CENTERED POPUP MESSAGE ========== */

    function showMessage(message, isError) {
        // Remove existing message box if any
        var existingMsgBox = document.getElementById('customMessageBox');
        if (existingMsgBox) {
            existingMsgBox.remove();
        }
        var existingOverlay = document.getElementById('customMessageOverlay');
        if (existingOverlay) {
            existingOverlay.remove();
        }

        // Create modal overlay
        var overlay = document.createElement('div');
        overlay.id = 'customMessageOverlay';
        overlay.style.position = 'fixed';
        overlay.style.top = '0';
        overlay.style.left = '0';
        overlay.style.width = '100%';
        overlay.style.height = '100%';
        overlay.style.backgroundColor = 'rgba(0, 0, 0, 0.5)';
        overlay.style.zIndex = '10000';
        overlay.style.display = 'flex';
        overlay.style.alignItems = 'center';
        overlay.style.justifyContent = 'center';

        // Create message box
        var msgBox = document.createElement('div');
        msgBox.id = 'customMessageBox';
        msgBox.style.backgroundColor = isError ? '#fff5f5' : '#f0fff4';
        msgBox.style.color = isError ? '#c62828' : '#2e7d32';
        msgBox.style.padding = '24px 32px';
        msgBox.style.borderRadius = '16px';
        msgBox.style.fontWeight = '600';
        msgBox.style.fontSize = '1.1rem';
        msgBox.style.textAlign = 'center';
        msgBox.style.minWidth = '280px';
        msgBox.style.maxWidth = '380px';
        msgBox.style.boxShadow = '0 20px 40px rgba(0,0,0,0.2)';
        msgBox.style.border = isError ? '2px solid #c62828' : '2px solid #2e7d32';
        msgBox.style.fontFamily = 'DM Sans, sans-serif';

        // Add icon
        var icon = document.createElement('div');
        icon.style.fontSize = '2.5rem';
        icon.style.marginBottom = '12px';
        icon.innerHTML = isError ? '⚠️' : '✅';
        msgBox.appendChild(icon);

        // Add message text
        var text = document.createElement('div');
        text.textContent = message;
        msgBox.appendChild(text);

        // Add OK button
        var okBtn = document.createElement('button');
        okBtn.textContent = 'OK';
        okBtn.style.marginTop = '20px';
        okBtn.style.padding = '8px 28px';
        okBtn.style.backgroundColor = isError ? '#c62828' : '#2e7d32';
        okBtn.style.color = 'white';
        okBtn.style.border = 'none';
        okBtn.style.borderRadius = '8px';
        okBtn.style.fontWeight = '600';
        okBtn.style.cursor = 'pointer';
        okBtn.style.fontFamily = 'DM Sans, sans-serif';
        okBtn.style.fontSize = '0.9rem';

        okBtn.onclick = function () {
            overlay.remove();
            // Redirect after OK if it's a login error
            if (isError && message.toLowerCase().includes('login')) {
                window.location.href = '/Home/Login';
            }
        };

        msgBox.appendChild(okBtn);
        overlay.appendChild(msgBox);
        document.body.appendChild(overlay);
    }

    function checkLogin(callback) {
        fetch('/Account/CheckLogin', {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json'
            }
        })
            .then(function (response) { return response.json(); })
            .then(function (data) {
                if (callback) callback(data.isLoggedIn);
            })
            .catch(function (error) {
                console.error('Login check error:', error);
                if (callback) callback(false);
            });
    }

    // Handle product card click (redirect to product page)
    function attachProductCardEvents() {
        var productCards = document.querySelectorAll('.product-card');

        productCards.forEach(function (card) {
            // Remove existing inline onclick if any
            card.removeAttribute('onclick');

            // Remove existing event listener to avoid duplicates
            if (card._clickListener) {
                card.removeEventListener('click', card._clickListener);
            }

            // Create new click handler
            var clickHandler = function (event) {
                // Ignore clicks on add button
                if (event.target.classList && event.target.classList.contains('add-btn')) {
                    return;
                }

                event.preventDefault();
                event.stopPropagation();

                var redirectUrl = card.getAttribute('data-product-url');
                if (!redirectUrl) {
                    console.warn('No data-product-url found for card');
                    return;
                }

                checkLogin(function (isLoggedIn) {
                    if (isLoggedIn) {
                        window.location.href = redirectUrl;
                    } else {
                        showMessage('Please login first to view product details.', true);
                    }
                });
            };

            card._clickListener = clickHandler;
            card.addEventListener('click', clickHandler);
        });
    }

    // Handle Add to Cart buttons
    function attachAddToCartEvents() {
        var addBtns = document.querySelectorAll('.add-btn');

        addBtns.forEach(function (btn) {
            // Remove existing listener to avoid duplicates
            if (btn._clickListener) {
                btn.removeEventListener('click', btn._clickListener);
            }

            var clickHandler = function (event) {
                event.preventDefault();
                event.stopPropagation();

                checkLogin(function (isLoggedIn) {
                    if (isLoggedIn) {
                        showMessage('Product added to cart!', false);
                        // TODO: Add your actual add to cart logic here
                    } else {
                        showMessage('Please login first to add items to cart.', true);
                    }
                });
            };

            btn._clickListener = clickHandler;
            btn.addEventListener('click', clickHandler);
        });
    }

    // Initial attach of all events
    function attachAllEvents() {
        attachProductCardEvents();
        attachAddToCartEvents();
    }

    attachAllEvents();

    // Re-attach events when DOM changes (for filtering)
    var observer = new MutationObserver(function () {
        attachAllEvents();
    });

    var productGrid = document.getElementById('productGrid');
    if (productGrid) {
        observer.observe(productGrid, { childList: true, subtree: true });
    }
});
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

        noResults.style.display = visible === 0 ? 'block' : 'none';
    }

    if (searchInput) searchInput.addEventListener('input', filterProducts);
    if (categoryFilter) categoryFilter.addEventListener('change', filterProducts);
    if (popularityFilter) popularityFilter.addEventListener('change', filterProducts);
    if (priceFilter) priceFilter.addEventListener('change', filterProducts);
});

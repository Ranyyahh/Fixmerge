// ============================================================
// AdminEnterprises.js (FIXED VERSION)
// ============================================================
// NOTE: Remove EXAMPLE_ENTERPRISES when backend is ready
// ============================================================

const EXAMPLE_ENTERPRISES = [
    { id: 'BQCU0110', name: 'Seresa Enterprise', rating: 4 },
    { id: 'BQCU0111', name: 'Linaya Enterprise', rating: 3 },
    { id: 'BQCU0112', name: 'Coco House Enterprise', rating: 5 },
    { id: 'BQCU0113', name: 'Tazyo Enterprise', rating: 2 },
    { id: 'BQCU0114', name: 'Bloom & Co', rating: 4 },
    { id: 'BQCU0115', name: "Nana's Kitchen", rating: 5 },
    { id: 'BQCU0116', name: 'GreenLeaf Trading', rating: 3 },
    { id: 'BQCU0117', name: 'SunRise Goods', rating: 4 },
    { id: 'BQCU0118', name: 'Metro Crafts', rating: 5 },
];

// ── DOM REFS ──
const searchInput = document.getElementById('searchInput');
const grid = document.getElementById('enterprisesGrid');
const countBadge = document.getElementById('countBadge');

// Safety check (prevents crash if missing in HTML)
if (!grid || !countBadge) {
    console.error("Missing required DOM elements (grid or countBadge).");
}

// ── RENDER SINGLE CARD ──
function renderCard(enterprise) {

    const rating = enterprise.rating ?? 0;

    const stars = Array.from({ length: 5 }, (_, i) =>
        `<span class="star${i < rating ? ' filled' : ''}">★</span>`
    ).join('');

    const avatar = enterprise.profileImg
        ? `<img src="${enterprise.profileImg}" alt="${enterprise.name}" />`
        : `<svg width="44" height="44" viewBox="0 0 24 24" fill="none"
               stroke="#1a1f4e" stroke-width="1.5"
               stroke-linecap="round" stroke-linejoin="round">
               <path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"/>
               <circle cx="12" cy="7" r="4"/>
           </svg>`;

    const card = document.createElement('div');
    card.className = 'enterprise-card';
    card.dataset.name = enterprise.name;
    card.dataset.id = enterprise.id;

    card.innerHTML = `
        <div class="card-avatar">${avatar}</div>
        <div class="card-name">${enterprise.name}</div>
        <div class="card-id">${enterprise.id}</div>
        <div class="card-stars">${stars}</div>
        <a href="/AdminPanel/AdminEditEntrep?enterpriseId=${enterprise.id}" class="view-btn">
            View Account
        </a>
    `;

    return card;
}

// ── RENDER ALL ──
function renderCards(enterprises) {
    grid.innerHTML = '';

    if (!enterprises || enterprises.length === 0) {
        showEmptyState();
        return;
    }

    enterprises.forEach(e => grid.appendChild(renderCard(e)));
    updateCount(enterprises.length);
}

function updateCount(n) {
    countBadge.textContent = `${n} ${n === 1 ? 'Enterprise' : 'Enterprises'}`;
}

function showEmptyState(query = '') {
    grid.innerHTML = '';

    const empty = document.createElement('div');
    empty.className = 'empty-state';

    empty.innerHTML = `
        <svg width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="currentColor"
             stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round">
            <circle cx="11" cy="11" r="8"/>
            <line x1="21" y1="21" x2="16.65" y2="16.65"/>
        </svg>
        <p>
            ${query
            ? `No enterprises found for "<strong>${query}</strong>"`
            : 'No enterprises found.'}
        </p>
    `;

    grid.appendChild(empty);
    updateCount(0);
}

// ── SEARCH ──
if (searchInput) {
    searchInput.addEventListener('input', function () {

        const query = this.value.trim().toLowerCase();
        const cards = grid.querySelectorAll('.enterprise-card');
        let visible = 0;

        cards.forEach(card => {
            const match =
                card.dataset.name.toLowerCase().includes(query) ||
                card.dataset.id.toLowerCase().includes(query);

            card.style.display = match ? '' : 'none';
            if (match) visible++;
        });

        updateCount(visible);

        if (visible === 0) {
            showEmptyState(this.value);
        } else {
            const empty = grid.querySelector('.empty-state');
            if (empty) empty.remove();
        }
    });
}

// ── INIT ──
renderCards(EXAMPLE_ENTERPRISES);
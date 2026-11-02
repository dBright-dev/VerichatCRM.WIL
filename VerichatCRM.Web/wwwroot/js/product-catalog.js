document.addEventListener('DOMContentLoaded', () => {
    const chips = document.querySelectorAll('.chip');
    const cards = Array.from(document.querySelectorAll('.product-card'));
    const grid = document.querySelector('.product-grid');
    const search = document.getElementById('catalogSearch');
    const sortSelect = document.getElementById('catalogSort');

    function filter() {
        const activeCat = document.querySelector('.chip.active')?.dataset.category || 'All';
        const query = search?.value.toLowerCase() || '';

        cards.forEach(card => {
            const matchesCat = activeCat === 'All' || card.dataset.category === activeCat;
            const matchesQuery = !query ||
                card.dataset.name.includes(query) ||
                card.dataset.sku.includes(query);
            card.style.display = (matchesCat && matchesQuery) ? '' : 'none';
        });
    }

    chips.forEach(chip => {
        chip.addEventListener('click', () => {
            chips.forEach(c => c.classList.remove('active'));
            chip.classList.add('active');
            filter();
        });
    });

    search?.addEventListener('input', filter);

    // The dropdown used to be markup only: no id, no values, no listener, so
    // choosing "Price High-Low" changed nothing on screen. Ordering the DOM here
    // rather than re-requesting the page keeps the chips and the search box alive -
    // sort and filter are independent, so the row you see is the current filter in
    // the chosen order.
    const comparators = {
        'name-asc': (a, b) => a.dataset.name.localeCompare(b.dataset.name),
        'name-desc': (a, b) => b.dataset.name.localeCompare(a.dataset.name),
        'price-asc': (a, b) => Number(a.dataset.price) - Number(b.dataset.price),
        'price-desc': (a, b) => Number(b.dataset.price) - Number(a.dataset.price),
        'stock-asc': (a, b) => Number(a.dataset.stock) - Number(b.dataset.stock),
    };

    sortSelect?.addEventListener('change', () => {
        const compare = comparators[sortSelect.value];
        if (!compare || !grid) return;

        const ordered = [...cards].sort(compare);
        ordered.forEach(card => grid.appendChild(card));
    });

    filter();
});
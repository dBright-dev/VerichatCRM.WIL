document.addEventListener('DOMContentLoaded', () => {
    const chips = document.querySelectorAll('.chip');
    const cards = document.querySelectorAll('.product-card');
    const search = document.getElementById('catalogSearch');

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
});
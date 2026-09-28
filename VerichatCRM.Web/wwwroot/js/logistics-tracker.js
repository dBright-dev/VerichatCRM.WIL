document.addEventListener('DOMContentLoaded', () => {
    const chips = document.querySelectorAll('.chip');
    const cards = document.querySelectorAll('.shipment-card');

    chips.forEach(chip => {
        chip.addEventListener('click', () => {
            chips.forEach(c => c.classList.remove('active'));
            chip.classList.add('active');

            const status = chip.dataset.status;
            cards.forEach(card => {
                card.style.display = (status === 'All' || card.dataset.status === status) ? '' : 'none';
            });
        });
    });
});
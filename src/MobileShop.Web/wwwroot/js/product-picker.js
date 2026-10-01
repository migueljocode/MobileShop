// Shared product-picker behaviour for the transaction Buy and Sell pages.
//
// On product change, shows the selected product's suggested catalog price and always sets the
// finished-price input to it (documented UX rule; the user can still override afterwards).
// The server remains the source of truth for the recorded price.
//
// Hooks (page-owned tag-helper controls):
//   [data-product-picker]          the product <select>
//   [data-finished-price-input]    the finished-price <input> (binds to Input.Price)
//   [data-suggested-price-display] the read-only suggested price display element
// Per-option: data-suggested-price (invariant culture, may be empty).
document.addEventListener('DOMContentLoaded', function () {
    var picker = document.querySelector('[data-product-picker]');
    var priceInput = document.querySelector('[data-finished-price-input]');
    var suggestedDisplay = document.querySelector('[data-suggested-price-display]');
    if (!picker || !priceInput) return;
    picker.addEventListener('change', function () {
        var option = picker.options[picker.selectedIndex];
        var suggested = option ? option.getAttribute('data-suggested-price') : '';
        if (suggestedDisplay) suggestedDisplay.textContent = suggested ? suggested : '—';
        if (suggested) priceInput.value = suggested;
    });
});

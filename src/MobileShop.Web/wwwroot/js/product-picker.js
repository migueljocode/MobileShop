// Shared searchable product picker for the transaction Buy and Sell pages.
// Keeps the selected product id in the form and preserves suggested-price autofill.
document.addEventListener('DOMContentLoaded', function () {
    document.querySelectorAll('[data-product-picker]').forEach(function (picker) {
        var searchInput = picker.querySelector('[data-product-search]');
        var valueInput = picker.querySelector('[data-product-value]');
        var results = picker.querySelector('[data-product-results]');
        var priceInput = document.querySelector('[data-finished-price-input]');
        var suggestedDisplay = picker.querySelector('[data-suggested-price-display]');
        var searchHandler = picker.getAttribute('data-search-handler');
        if (!searchInput || !valueInput || !results || !priceInput || !searchHandler) return;

        var timer;

        function formatPrice(value) {
            return value ? Number(value).toLocaleString('en-US') + ' IRR' : '—';
        }

        function setSelection(option) {
            valueInput.value = String(option.productId);
            searchInput.value = option.label;
            results.classList.add('d-none');
            searchInput.setAttribute('aria-expanded', 'false');

            var suggested = option.suggestedPrice == null ? '' : String(option.suggestedPrice);
            if (suggestedDisplay) suggestedDisplay.textContent = formatPrice(suggested);
            if (suggested) priceInput.value = suggested;
        }

        function renderResults(options) {
            results.replaceChildren();
            options.forEach(function (option) {
                var button = document.createElement('button');
                button.type = 'button';
                button.className = 'list-group-item list-group-item-action';
                button.setAttribute('role', 'option');
                button.textContent = option.label;
                button.addEventListener('click', function () {
                    setSelection(option);
                });
                results.appendChild(button);
            });
            results.classList.toggle('d-none', options.length === 0);
            searchInput.setAttribute('aria-expanded', options.length > 0 ? 'true' : 'false');
        }

        async function search(query) {
            var url = new URL(window.location.href);
            url.searchParams.set('handler', searchHandler);
            if (query) url.searchParams.set('q', query);
            else url.searchParams.delete('q');

            var response = await fetch(url, { headers: { Accept: 'application/json' } });
            if (!response.ok) throw new Error('Search failed.');

            var products = await response.json();
            renderResults(products.map(function (product) {
                var label = product.type + ': ' + product.name + ' — ' + product.identifier;
                if (product.color) label += ' — ' + product.color;
                if (product.partNumberLabel && product.partNumberLabel !== 'N/A') label += ' — ' + product.partNumberLabel;
                return {
                    productId: product.productId,
                    label: label,
                    suggestedPrice: product.suggestedPrice
                };
            }));
        }

        searchInput.addEventListener('input', function () {
            clearTimeout(timer);
            valueInput.value = '0';
            timer = setTimeout(function () {
                search(searchInput.value.trim()).catch(function () {
                    results.replaceChildren();
                    results.classList.add('d-none');
                });
            }, 200);
        });

        searchInput.addEventListener('focus', function () {
            if (!searchInput.value.trim()) {
                search('').catch(function () { });
            }
        });

        picker.querySelectorAll('[data-product-option]').forEach(function (button) {
            button.addEventListener('click', function () {
                setSelection({
                    productId: button.dataset.productId,
                    label: button.textContent,
                    suggestedPrice: button.dataset.suggestedPrice || ''
                });
            });
        });

        var selected = picker.querySelector('[data-product-option][data-selected="true"]');
        if (selected) {
            setSelection({
                productId: selected.dataset.productId,
                label: selected.textContent,
                suggestedPrice: selected.dataset.suggestedPrice || ''
            });
        }
    });
});

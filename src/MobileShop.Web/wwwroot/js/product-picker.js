// Shared searchable product picker for the transaction Buy and Sell pages.
// Keeps the selected product id in the form and preserves suggested-price autofill.
document.addEventListener('DOMContentLoaded', function () {
    document.querySelectorAll('[data-product-picker]').forEach(function (picker) {
        var searchInput = picker.querySelector('[data-product-search]');
        var valueInput = picker.querySelector('[data-product-value]');
        var results = picker.querySelector('[data-product-results]');
        var typeFilter = picker.querySelector('[data-product-type-filter]');
        var manufacturerFilter = picker.querySelector('[data-product-manufacturer-filter]');
        var modelFilter = picker.querySelector('[data-product-model-filter]');
        var priceInput = document.querySelector('[data-finished-price-input]');
        var suggestedDisplay = picker.querySelector('[data-suggested-price-display]');
        var searchHandler = picker.getAttribute('data-search-handler');
        if (!searchInput || !valueInput || !results || !priceInput || !searchHandler) return;

        var timer;
        var catalog = Array.from(results.querySelectorAll('[data-product-option]')).map(function (button) {
            return {
                type: button.dataset.productType || '',
                manufacturerId: button.dataset.manufacturerId || '',
                manufacturerName: button.dataset.manufacturerName || '',
                modelId: button.dataset.modelId || '',
                modelName: button.dataset.modelName || ''
            };
        });
        var selectedSuggestedPrice = '';

        function clearSuggestedPrice() {
            if (selectedSuggestedPrice && priceInput.value === selectedSuggestedPrice) priceInput.value = '';
            selectedSuggestedPrice = '';
            if (suggestedDisplay) suggestedDisplay.textContent = '—';
        }

        function filteredCatalog() {
            return catalog.filter(function (product) {
                return (!typeFilter || !typeFilter.value || product.type === typeFilter.value) &&
                    (!manufacturerFilter || !manufacturerFilter.value || product.manufacturerId === manufacturerFilter.value);
            });
        }

        function populateManufacturers() {
            if (!manufacturerFilter) return;
            var selectedManufacturer = manufacturerFilter.value;
            var manufacturers = new Map();
            catalog.filter(function (product) {
                return !typeFilter || !typeFilter.value || product.type === typeFilter.value;
            }).forEach(function (product) {
                if (product.manufacturerId && product.manufacturerName) {
                    manufacturers.set(product.manufacturerId, product.manufacturerName);
                }
            });

            manufacturerFilter.replaceChildren(new Option('All manufacturers', ''));
            Array.from(manufacturers.entries())
                .sort(function (left, right) { return left[1].localeCompare(right[1]); })
                .forEach(function (manufacturer) {
                    manufacturerFilter.appendChild(new Option(manufacturer[1], manufacturer[0]));
                });
            if (manufacturers.has(selectedManufacturer)) manufacturerFilter.value = selectedManufacturer;
        }

        function populateModels() {
            if (!modelFilter) return;
            var models = new Map();
            filteredCatalog().forEach(function (product) {
                if (product.modelId && product.modelName) {
                    var label = product.modelName;
                    if ((!typeFilter || !typeFilter.value) && product.type) label += ' (' + product.type + ')';
                    models.set(product.modelId, label);
                }
            });

            modelFilter.replaceChildren(new Option('All models', ''));
            Array.from(models.entries())
                .sort(function (left, right) { return left[1].localeCompare(right[1]); })
                .forEach(function (model) {
                    modelFilter.appendChild(new Option(model[1], model[0]));
                });
            modelFilter.disabled = !manufacturerFilter || !manufacturerFilter.value;
        }

        function clearSelection() {
            valueInput.value = '0';
            searchInput.value = '';
            clearSuggestedPrice();
            results.classList.add('d-none');
            searchInput.setAttribute('aria-expanded', 'false');
        }

        function refreshFilteredResults() {
            clearSelection();
            search('').catch(function () {
                results.replaceChildren();
                results.classList.add('d-none');
            });
        }

        function formatPrice(value) {
            return value ? Number(value).toLocaleString('en-US') + ' IRR' : '—';
        }

        function setSelection(option) {
            valueInput.value = String(option.productId);
            searchInput.value = option.label;
            results.classList.add('d-none');
            searchInput.setAttribute('aria-expanded', 'false');

            var suggested = option.suggestedPrice == null ? '' : String(option.suggestedPrice);
            selectedSuggestedPrice = suggested;
            if (suggestedDisplay) suggestedDisplay.textContent = formatPrice(suggested);
            if (suggested) priceInput.value = suggested;
        }

        document.addEventListener('product-created', function (event) {
            var option = event.detail;
            if (option && option.productId && option.label) setSelection(option);
        });

        function renderResults(options) {
            results.replaceChildren();
            options.forEach(function (option) {
                var button = document.createElement('button');
                button.type = 'button';
                button.className = 'list-group-item list-group-item-action';
                button.setAttribute('role', 'option');
                button.textContent = option.label;
                button.addEventListener('click', function () { setSelection(option); });
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
            if (typeFilter && typeFilter.value) url.searchParams.set('type', typeFilter.value);
            else url.searchParams.delete('type');
            if (manufacturerFilter && manufacturerFilter.value) url.searchParams.set('manufacturerId', manufacturerFilter.value);
            else url.searchParams.delete('manufacturerId');
            if (modelFilter && modelFilter.value) url.searchParams.set('modelId', modelFilter.value);
            else url.searchParams.delete('modelId');

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
            clearSuggestedPrice();
            timer = setTimeout(function () {
                search(searchInput.value.trim()).catch(function () {
                    results.replaceChildren();
                    results.classList.add('d-none');
                });
            }, 200);
        });

        searchInput.addEventListener('focus', function () {
            if (!searchInput.value.trim()) search('').catch(function () { });
        });

        if (typeFilter && manufacturerFilter && modelFilter) {
            Array.from(new Set(catalog.map(function (product) { return product.type; }).filter(Boolean)))
                .sort(function (left, right) { return left.localeCompare(right); })
                .forEach(function (type) { typeFilter.appendChild(new Option(type, type)); });
            populateManufacturers();
            populateModels();

            typeFilter.addEventListener('change', function () {
                manufacturerFilter.value = '';
                modelFilter.value = '';
                populateManufacturers();
                populateModels();
                refreshFilteredResults();
            });
            manufacturerFilter.addEventListener('change', function () {
                modelFilter.value = '';
                populateModels();
                refreshFilteredResults();
            });
            modelFilter.addEventListener('change', refreshFilteredResults);
        }

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
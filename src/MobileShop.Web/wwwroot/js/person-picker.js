// Shared searchable person picker for the transaction Buy and Sell pages.
document.addEventListener('DOMContentLoaded', function () {
    document.querySelectorAll('[data-person-picker]').forEach(function (picker) {
        var searchInput = picker.querySelector('[data-person-search]');
        var valueInput = picker.querySelector('[data-person-value]');
        var results = picker.querySelector('[data-person-results]');
        var createButton = picker.querySelector('[data-person-create]');
        var modal = picker.querySelector('[data-person-modal]');
        var modalError = picker.querySelector('[data-person-modal-error]');
        var saveButton = picker.querySelector('[data-person-save]');
        var searchHandler = picker.getAttribute('data-search-handler');
        var createHandler = picker.getAttribute('data-create-handler');
        if (!searchInput || !valueInput || !results || !createButton || !modal || !modalError || !saveButton) return;

        var modalInstance = bootstrap.Modal.getOrCreateInstance(modal);
        var timer;

        function setSelection(id, label) {
            valueInput.value = String(id);
            searchInput.value = label;
            results.classList.add('d-none');
            searchInput.setAttribute('aria-expanded', 'false');
            picker.querySelector('[data-person-error]').textContent = '';
        }

        function renderResults(options) {
            results.replaceChildren();
            options.forEach(function (option) {
                var button = document.createElement('button');
                button.type = 'button';
                button.className = 'list-group-item list-group-item-action';
                button.setAttribute('role', 'option');
                button.dataset.personId = option.id;
                button.textContent = option.label;
                button.addEventListener('click', function () {
                    setSelection(option.id, option.label);
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
            renderResults(await response.json());
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

        createButton.addEventListener('click', function () {
            modalError.classList.add('d-none');
            modalError.textContent = '';
            picker.querySelectorAll('[data-person-field]').forEach(function (field) {
                field.value = field.tagName === 'SELECT' ? field.options[0].value : '';
            });
            modalInstance.show();
        });

        saveButton.addEventListener('click', async function () {
            modalError.classList.add('d-none');
            modalError.textContent = '';
            saveButton.disabled = true;
            try {
                var data = new FormData();
                picker.querySelectorAll('[data-person-field]').forEach(function (field) {
                    if (field.name) data.append(field.name, field.value);
                });
                var token = document.querySelector('input[name="__RequestVerificationToken"]');
                if (token) data.append('__RequestVerificationToken', token.value);

                var url = new URL(window.location.href);
                url.searchParams.set('handler', createHandler);
                var response = await fetch(url, { method: 'POST', body: data, headers: { Accept: 'application/json' } });
                var result = await response.json();
                if (!response.ok || !result.succeeded || !result.option) {
                    throw new Error(result.error || 'The person could not be created.');
                }

                setSelection(result.option.id, result.option.name);
                modalInstance.hide();
            } catch (error) {
                modalError.textContent = error.message || 'The person could not be created.';
                modalError.classList.remove('d-none');
            } finally {
                saveButton.disabled = false;
            }
        });

        picker.querySelectorAll('[data-person-results] [data-person-id]').forEach(function (button) {
            button.addEventListener('click', function () {
                setSelection(button.dataset.personId, button.textContent);
            });
        });
    });
});

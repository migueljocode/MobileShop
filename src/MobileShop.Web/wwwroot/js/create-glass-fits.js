// Create glass: the "Compatible phones" list, its add popup and the inline manufacturer/model creators.
(function () {
    'use strict';

    document.addEventListener('DOMContentLoaded', function () {
        var form = document.getElementById('productCreateForm');
        var list = document.getElementById('glassFitList');
        var template = document.getElementById('glassFitTemplate');
        var emptyText = document.getElementById('glassFitEmpty');
        var liveRegion = document.getElementById('glassFitLive');
        var validation = document.getElementById('glassFitValidation');
        var openButton = document.getElementById('openCompatiblePhoneModal');
        var modalEl = document.getElementById('addCompatiblePhoneModal');
        var phoneManufacturer = document.getElementById('compatiblePhoneManufacturer');
        var phoneModel = document.getElementById('compatiblePhoneModel');
        var popupError = document.getElementById('compatiblePhoneError');
        var addButton = document.getElementById('addCompatiblePhoneBtn');
        var newManufacturerRow = document.getElementById('newPhoneManufacturerRow');
        var newModelRow = document.getElementById('newPhoneModelRow');
        var glassManufacturer = document.getElementById('glassManufacturerSelect');
        var antiforgery = document.querySelector('input[name="__RequestVerificationToken"]');

        if (!form || !list || !template || !emptyText || !liveRegion || !validation || !openButton ||
            !modalEl || !phoneManufacturer || !phoneModel || !popupError || !addButton ||
            !newManufacturerRow || !newModelRow || !glassManufacturer) {
            return;
        }

        function items() {
            return Array.prototype.slice.call(list.querySelectorAll('.glass-fit-item'));
        }

        function selectedIds() {
            return items().map(function (item) {
                return item.querySelector('input[name="Input.CompatibleModelIds"]').value;
            });
        }

        function refreshEmpty() {
            emptyText.hidden = items().length > 0;
        }

        function announce(message) {
            liveRegion.textContent = '';
            liveRegion.textContent = message;
        }

        function setPopupError(message) {
            popupError.textContent = message || '';
            popupError.classList.toggle('d-none', !message);
        }

        function setValidation(message) {
            validation.textContent = message || '';
        }

        function selectedText(select) {
            var option = select.options[select.selectedIndex];
            return option ? option.text.trim() : '';
        }

        function setRowOpen(row, open) {
            row.classList.toggle('d-none', !open);
            if (open) row.querySelector('input').focus();
        }

        function closeInlineRows() {
            newManufacturerRow.classList.add('d-none');
            newModelRow.classList.add('d-none');
            document.getElementById('newPhoneManufacturerName').value = '';
            document.getElementById('newPhoneModelName').value = '';
        }

        function addFit(id, label) {
            var fragment = template.content.cloneNode(true);
            var item = fragment.querySelector('.glass-fit-item');
            item.querySelector('[data-fit-label]').textContent = label;
            item.querySelector('input[name="Input.CompatibleModelIds"]').value = id;
            var remove = item.querySelector('[data-fit-remove]');
            remove.setAttribute('aria-label', 'Remove ' + label);
            remove.title = 'Remove ' + label;
            list.appendChild(fragment);
            refreshEmpty();
            setValidation('');
            announce(label + ' added');
        }

        async function loadPhoneModels(manufacturerId) {
            phoneModel.disabled = true;
            addButton.disabled = true;
            if (!manufacturerId) {
                phoneModel.replaceChildren(new Option('-- Select manufacturer first --', ''));
                return;
            }

            phoneModel.replaceChildren(new Option('Loading...', ''));
            var response = await fetch('?handler=Models&manufacturerId=' + encodeURIComponent(manufacturerId), {
                headers: { Accept: 'application/json' }
            });
            if (!response.ok) throw new Error('Could not load phone models.');
            var models = await response.json();
            if (phoneManufacturer.value !== String(manufacturerId)) return; // the manufacturer changed meanwhile

            phoneModel.replaceChildren(new Option('-- Select model --', ''));
            models.forEach(function (model) {
                phoneModel.appendChild(new Option(model.name, model.id));
            });
            phoneModel.disabled = false;
        }

        async function postCreate(handler, fields) {
            var headers = { 'Content-Type': 'application/x-www-form-urlencoded; charset=UTF-8' };
            if (antiforgery) headers.RequestVerificationToken = antiforgery.value;
            var response = await fetch('?handler=' + handler, {
                method: 'POST',
                headers: headers,
                body: new URLSearchParams(fields)
            });
            var result = await response.json();
            if (!response.ok) throw new Error(result.error || 'Could not save.');
            return result;
        }

        refreshEmpty();

        list.addEventListener('click', function (event) {
            var button = event.target.closest('[data-fit-remove]');
            if (!button) return;

            var item = button.closest('.glass-fit-item');
            var label = item.querySelector('[data-fit-label]').textContent;
            var index = items().indexOf(item);
            item.remove();
            refreshEmpty();

            var remaining = items();
            var next = remaining[index] || remaining[index - 1];
            (next ? next.querySelector('[data-fit-remove]') : openButton).focus();
            announce(label + ' removed');
        });

        phoneManufacturer.addEventListener('change', function () {
            setPopupError('');
            closeInlineRows();
            loadPhoneModels(phoneManufacturer.value).catch(function (error) { setPopupError(error.message); });
        });

        phoneModel.addEventListener('change', function () {
            addButton.disabled = !phoneModel.value;
        });

        modalEl.addEventListener('show.bs.modal', function () {
            setPopupError('');
            if (phoneManufacturer.value) {
                loadPhoneModels(phoneManufacturer.value).catch(function (error) { setPopupError(error.message); });
            }
        });

        modalEl.addEventListener('hidden.bs.modal', function () {
            setPopupError('');
            closeInlineRows();
            phoneModel.value = '';
            addButton.disabled = true;
        });

        modalEl.addEventListener('keydown', function (event) {
            if (event.key === 'Enter' && event.target.tagName === 'SELECT' && !addButton.disabled) {
                event.preventDefault();
                addButton.click();
            }
        });

        addButton.addEventListener('click', function () {
            var id = phoneModel.value;
            if (!id) return;
            if (selectedIds().indexOf(id) !== -1) {
                setPopupError('That phone is already in the list.');
                return;
            }
            addFit(id, selectedText(phoneManufacturer) + ' ' + selectedText(phoneModel));
            bootstrap.Modal.getOrCreateInstance(modalEl).hide();
        });

        document.getElementById('showNewPhoneManufacturer').addEventListener('click', function () {
            setRowOpen(newManufacturerRow, true);
        });

        document.getElementById('showNewPhoneModel').addEventListener('click', function () {
            if (!phoneManufacturer.value) {
                setPopupError('Select a manufacturer first.');
                return;
            }
            setRowOpen(newModelRow, true);
        });

        document.getElementById('saveNewPhoneManufacturer').addEventListener('click', async function () {
            var input = document.getElementById('newPhoneManufacturerName');
            var name = input.value.trim();
            if (!name) return;
            try {
                var created = await postCreate('CreateManufacturer', { name: name });
                phoneManufacturer.appendChild(new Option(created.name, created.id, true, true));
                glassManufacturer.appendChild(new Option(created.name, created.id));
                newManufacturerRow.classList.add('d-none');
                input.value = '';
                setPopupError('');
                await loadPhoneModels(String(created.id));
            } catch (error) {
                setPopupError(error.message);
            }
        });

        document.getElementById('saveNewPhoneModel').addEventListener('click', async function () {
            var input = document.getElementById('newPhoneModelName');
            var name = input.value.trim();
            if (!name) return;
            if (!phoneManufacturer.value) {
                setPopupError('Select a manufacturer first.');
                return;
            }
            try {
                var created = await postCreate('CreateModel', { manufacturerId: phoneManufacturer.value, name: name });
                phoneModel.appendChild(new Option(created.name, created.id, true, true));
                phoneModel.disabled = false;
                addButton.disabled = false;
                newModelRow.classList.add('d-none');
                input.value = '';
                setPopupError('');
            } catch (error) {
                setPopupError(error.message);
            }
        });

        document.getElementById('saveGlassManufacturerBtn').addEventListener('click', async function () {
            var input = document.getElementById('newGlassManufacturerName');
            var name = input.value.trim();
            if (!name) return;
            try {
                var created = await postCreate('CreateManufacturer', { name: name });
                glassManufacturer.appendChild(new Option(created.name, created.id, true, true));
                input.value = '';
                bootstrap.Modal.getOrCreateInstance(document.getElementById('addGlassManufacturerModal')).hide();
            } catch (error) {
                window.alert(error.message);
            }
        });

        form.addEventListener('submit', function (event) {
            if (items().length > 0) {
                setValidation('');
                return;
            }
            event.preventDefault();
            setValidation('Add at least one compatible phone.');
            openButton.focus();
        });
    });
})();

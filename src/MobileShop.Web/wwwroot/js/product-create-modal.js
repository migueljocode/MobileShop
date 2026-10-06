document.addEventListener('DOMContentLoaded', function () {
    var modal = document.getElementById('create-product-modal');
    var typeSelect = document.getElementById('create-product-type');
    var formHost = document.getElementById('create-product-form-host');
    var form = document.getElementById('create-product-form');
    var status = document.getElementById('create-product-status');
    if (!modal || !typeSelect || !formHost || !form || !status) return;

    var registry = Object.freeze({
        phone: { formHandler: 'CreateProductForm', postHandler: 'CreatePhone' },
        appleid: { formHandler: 'CreateProductForm', postHandler: 'CreateAppleId' },
        glass: { formHandler: 'CreateProductForm', postHandler: 'CreateGlass' },
        tablet: { formHandler: 'CreateProductForm', postHandler: 'CreateTablet', modelsHandler: 'CreateTabletModels' },
        smartwatch: { formHandler: 'CreateProductForm', postHandler: 'CreateSmartWatch', modelsHandler: 'CreateSmartWatchModels' },
        laptop: { formHandler: 'CreateProductForm', postHandler: 'CreateLaptop', modelsHandler: 'CreateLaptopModels' },
        cable: { formHandler: 'CreateProductForm', postHandler: 'CreateCable', modelsHandler: 'CreateAccessoryModels', category: 'Cable' },
        charger: { formHandler: 'CreateProductForm', postHandler: 'CreateCharger', modelsHandler: 'CreateAccessoryModels', category: 'Charger' },
        powerbank: { formHandler: 'CreateProductForm', postHandler: 'CreatePowerBank', modelsHandler: 'CreateAccessoryModels', category: 'PowerBank' },
        portablestorage: { formHandler: 'CreateProductForm', postHandler: 'CreatePortableStorage', modelsHandler: 'CreateAccessoryModels', category: 'PortableStorage' },
        case: { formHandler: 'CreateProductForm', postHandler: 'CreateCase', modelsHandler: 'CreateCaseModels' }
    });
    Object.keys(registry).forEach(function (key) {
        if (!typeSelect.querySelector('option[value="' + key + '"]')) {
            var option = new Option(key === 'smartwatch' ? 'Smart Watch' : key === 'appleid' ? 'Apple ID' : key === 'powerbank' ? 'Power Bank' : key === 'portablestorage' ? 'Portable Storage' : key.charAt(0).toUpperCase() + key.slice(1), key);
            typeSelect.appendChild(option);
        }
    });

    function setStatus(message, isError) {
        status.textContent = message || '';
        status.classList.toggle('text-danger', Boolean(isError));
        status.classList.toggle('text-muted', !isError);
    }

    async function loadForm(type) {
        formHost.replaceChildren();
        if (!type || !registry[type]) return;
        var url = new URL(window.location.href);
        url.searchParams.set('handler', registry[type].formHandler);
        url.searchParams.set('type', type);
        var response = await fetch(url, { headers: { Accept: 'text/html' } });
        if (!response.ok) throw new Error('The product form could not be loaded.');
        formHost.innerHTML = await response.text();
        var root = formHost.firstElementChild;
        if (!root) return;

        if (type === 'phone') {
            var manufacturer = root.querySelector('[data-phone-manufacturer]');
            var model = root.querySelector('[data-phone-model]');
            if (manufacturer && model) {
                manufacturer.addEventListener('change', async function () {
                    model.replaceChildren(new Option('-- Select model --', ''));
                    if (!manufacturer.value) return;
                    var modelUrl = new URL(window.location.href);
                    modelUrl.searchParams.set('handler', 'CreatePhoneModels');
                    modelUrl.searchParams.set('manufacturerId', manufacturer.value);
                    var response = await fetch(modelUrl, { headers: { Accept: 'application/json' } });
                    if (!response.ok) return;
                    for (var item of await response.json()) model.appendChild(new Option(item.name, item.id));
                });
            }
        }

        if (type === 'tablet' || type === 'smartwatch' || type === 'laptop') {
            var deviceManufacturer = root.querySelector('[data-device-manufacturer]');
            var deviceModel = root.querySelector('[data-device-model]');
            if (deviceManufacturer && deviceModel) {
                deviceManufacturer.addEventListener('change', async function () {
                    deviceModel.replaceChildren(new Option('-- Select model --', ''));
                    if (!deviceManufacturer.value) return;
                    var modelUrl = new URL(window.location.href);
                    modelUrl.searchParams.set('handler', registry[type].modelsHandler);
                    modelUrl.searchParams.set('manufacturerId', deviceManufacturer.value);
                    var response = await fetch(modelUrl, { headers: { Accept: 'application/json' } });
                    if (!response.ok) return;
                    for (var item of await response.json()) deviceModel.appendChild(new Option(item.name, item.id));
                });
            }
        }

        if (registry[type].modelsHandler && (type === 'cable' || type === 'charger' || type === 'powerbank' || type === 'portablestorage')) {
            var accessoryManufacturer = root.querySelector('[data-accessory-manufacturer]');
            var accessoryModel = root.querySelector('[data-accessory-model]');
            if (accessoryManufacturer && accessoryModel) {
                accessoryManufacturer.addEventListener('change', async function () {
                    accessoryModel.replaceChildren(new Option('-- Select model --', ''));
                    if (!accessoryManufacturer.value) return;
                    var modelUrl = new URL(window.location.href);
                    modelUrl.searchParams.set('handler', registry[type].modelsHandler);
                    modelUrl.searchParams.set('manufacturerId', accessoryManufacturer.value);
                    modelUrl.searchParams.set('category', registry[type].category);
                    var response = await fetch(modelUrl, { headers: { Accept: 'application/json' } });
                    if (!response.ok) return;
                    for (var item of await response.json()) accessoryModel.appendChild(new Option(item.name, item.id));
                });
            }
        }

        if (type === 'case') {
            var compatibleManufacturer = root.querySelector('[data-case-compatible-manufacturer]');
            var compatibleModels = root.querySelector('[data-case-compatible-models]');
            if (compatibleManufacturer && compatibleModels) {
                compatibleManufacturer.addEventListener('change', async function () {
                    compatibleModels.replaceChildren();
                    if (!compatibleManufacturer.value) return;
                    var modelUrl = new URL(window.location.href);
                    modelUrl.searchParams.set('handler', registry[type].modelsHandler);
                    modelUrl.searchParams.set('manufacturerId', compatibleManufacturer.value);
                    var response = await fetch(modelUrl, { headers: { Accept: 'application/json' } });
                    if (!response.ok) return;
                    for (var item of await response.json()) compatibleModels.appendChild(new Option(item.name, item.id));
                });
            }
        }

        if (type === 'glass') {
            var manufacturer = root.querySelector('[data-glass-compatible-manufacturer]');
            var model = root.querySelector('[data-glass-compatible-model]');
            if (manufacturer && model) {
                manufacturer.addEventListener('change', async function () {
                    model.replaceChildren(new Option('-- Select model --', ''));
                    if (!manufacturer.value) return;
                    var modelUrl = new URL(window.location.href);
                    modelUrl.searchParams.set('handler', 'CreateGlassModels');
                    modelUrl.searchParams.set('manufacturerId', manufacturer.value);
                    var response = await fetch(modelUrl, { headers: { Accept: 'application/json' } });
                    if (!response.ok) return;
                    for (var item of await response.json()) model.appendChild(new Option(item.name, item.id));
                });
            }
        }
    }

    typeSelect.addEventListener('change', function () {
        setStatus('', false);
        loadForm(typeSelect.value).catch(function (error) { setStatus(error.message, true); });
    });

    form.addEventListener('submit', async function (event) {
        event.preventDefault();
        var type = typeSelect.value;
        if (!type || !registry[type] || !formHost.firstElementChild) {
            setStatus('Select a product type first.', true);
            return;
        }

        setStatus('Saving…', false);
        var submit = form.querySelector('[type="submit"]');
        if (submit) submit.disabled = true;

        try {
            var postUrl = new URL(window.location.href);
            postUrl.searchParams.set('handler', registry[type].postHandler);
            var response = await fetch(postUrl, {
                method: 'POST',
                body: new FormData(form),
                headers: { Accept: 'application/json' }
            });
            var result = await response.json();
            if (!response.ok || !result.productId) throw new Error(result.error || 'The product could not be created.');
            document.dispatchEvent(new CustomEvent('product-created', { detail: result }));
            bootstrap.Modal.getOrCreateInstance(modal).hide();
            form.reset();
            formHost.replaceChildren();
            typeSelect.value = '';
            setStatus('', false);
        } catch (error) {
            setStatus(error.message, true);
        } finally {
            if (submit) submit.disabled = false;
        }
    });

    modal.addEventListener('show.bs.modal', function () {
        setStatus('', false);
        typeSelect.value = '';
        formHost.replaceChildren();
    });
});
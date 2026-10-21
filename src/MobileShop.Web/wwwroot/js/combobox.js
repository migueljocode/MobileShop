document.addEventListener('DOMContentLoaded', function () {
    var MENU_ID = 0;

    function textOf(option) {
        return (option.textContent || '').replace(/\s+/g, ' ').trim();
    }

    /* The label of a field: asp-for renders it with the field name and
       underscores, a hand written id may be something else entirely. */
    function findLabel(select) {
        var candidates = [];
        if (select.id) candidates.push(select.id);
        if (select.name) {
            candidates.push(select.name);
            candidates.push(select.name.replace(/\./g, '_'));
        }
        for (var i = 0; i < candidates.length; i++) {
            var label = document.querySelector('label[for="' + candidates[i] + '"]');
            if (label) return label;
        }
        return null;
    }

    /* Turns a native <select> into the app's own combo box: a themed control
       that opens the same floating menu the searchable pickers use. The native
       element stays in the form (visually hidden) so posting, validation and
       every change handler keep working untouched. */
    function enhance(select) {
        if (select.dataset.combobox === 'ready') return;
        if (select.multiple || select.size > 1) return;
        select.dataset.combobox = 'ready';

        var wrapper = document.createElement('div');
        wrapper.className = 'combobox';
        select.parentNode.insertBefore(wrapper, select);
        wrapper.appendChild(select);

        var control = document.createElement('button');
        control.type = 'button';
        control.className = 'combobox__control';
        control.setAttribute('role', 'combobox');
        control.setAttribute('aria-haspopup', 'listbox');
        control.setAttribute('aria-expanded', 'false');
        control.setAttribute('aria-controls', 'combobox-menu-' + ++MENU_ID);

        var value = document.createElement('span');
        value.className = 'combobox__value';
        control.appendChild(value);
        wrapper.insertBefore(control, select);

        var menu = document.createElement('div');
        menu.className = 'combobox__menu';
        menu.id = control.getAttribute('aria-controls');
        menu.setAttribute('role', 'listbox');
        menu.hidden = true;
        wrapper.appendChild(menu);

        select.classList.add('combobox__native');
        select.setAttribute('tabindex', '-1');
        select.setAttribute('aria-hidden', 'true');

        // Keep the visible label as the control's accessible name. The label's
        // "for" may be the field name with dots turned into underscores, and
        // the select may carry its own id, so both are tried.
        var label = findLabel(select);
        if (label && label.textContent.trim()) control.setAttribute('aria-label', label.textContent.trim());
        else if (select.getAttribute('aria-label')) control.setAttribute('aria-label', select.getAttribute('aria-label'));

        var options = [];
        var activeIndex = -1;
        // A hidden ruler that keeps the control wide enough for the longest
        // option. A min-width would ignore a narrow column, so the width is
        // measured instead: the ruler is in flow (it grows a shrink-to-fit
        // column) but capped, so it never pushes into the next field.
        var measure = document.createElement('span');
        measure.className = 'combobox__measure';
        measure.setAttribute('aria-hidden', 'true');
        wrapper.appendChild(measure);

        function syncValue() {
            var option = select.selectedOptions && select.selectedOptions[0];
            value.textContent = option ? textOf(option) : '';
            value.classList.toggle('is-placeholder', !option || (option.value === '' && option.disabled));
            control.disabled = select.disabled;
        }

        function fitLongestOption() {
            var longest = '';
            for (var i = 0; i < select.options.length; i++) {
                var text = textOf(select.options[i]);
                if (text.length > longest.length) longest = text;
            }
            measure.textContent = longest;
        }

        function build() {
            menu.textContent = '';
            options = Array.prototype.slice.call(select.options);
            options.forEach(function (option, index) {
                var item = document.createElement('div');
                item.className = 'combobox__option';
                item.id = menu.id + '-option-' + index;
                item.setAttribute('role', 'option');
                item.setAttribute('aria-selected', option.selected ? 'true' : 'false');
                item.dataset.index = String(index);
                item.textContent = textOf(option);
                if (option.disabled) item.setAttribute('aria-disabled', 'true');
                menu.appendChild(item);
            });
            activeIndex = options.findIndex(function (option) { return option.selected; });
            fitLongestOption();
        }

        function highlight() {
            var items = menu.querySelectorAll('.combobox__option');
            items.forEach(function (item) { item.classList.remove('is-active'); });
            if (activeIndex < 0 || !items[activeIndex]) {
                control.removeAttribute('aria-activedescendant');
                return;
            }
            items[activeIndex].classList.add('is-active');
            control.setAttribute('aria-activedescendant', items[activeIndex].id);
            items[activeIndex].scrollIntoView({ block: 'nearest' });
        }

        function place() {
            menu.classList.remove('is-above');
            var rect = control.getBoundingClientRect();
            var below = window.innerHeight - rect.bottom;
            var above = rect.top;
            if (below < 200 && above > below) menu.classList.add('is-above');
        }

        function open() {
            if (control.disabled) return;
            build();
            menu.hidden = false;
            wrapper.classList.add('is-open');
            control.setAttribute('aria-expanded', 'true');
            place();
            highlight();
        }

        function close() {
            if (menu.hidden) return;
            menu.hidden = true;
            wrapper.classList.remove('is-open');
            control.setAttribute('aria-expanded', 'false');
            control.removeAttribute('aria-activedescendant');
        }

        function move(step) {
            if (!options.length) return;
            var count = options.length;
            var next = activeIndex;
            for (var i = 0; i < count; i++) {
                next = (next + step + count) % count;
                if (!options[next].disabled) break;
            }
            activeIndex = next;
            highlight();
        }

        function choose(index) {
            if (index < 0 || !options[index] || options[index].disabled) return;
            select.selectedIndex = index;
            // The real select still raises the change, so inline onchange
            // handlers (auto-submitting filter forms) keep firing.
            select.dispatchEvent(new Event('change', { bubbles: true }));
            syncValue();
            close();
        }

        control.addEventListener('click', function () {
            if (menu.hidden) open(); else close();
        });

        control.addEventListener('keydown', function (event) {
            if (event.key === 'ArrowDown' || event.key === 'ArrowUp') {
                event.preventDefault();
                if (menu.hidden) { open(); } else { move(event.key === 'ArrowDown' ? 1 : -1); }
                return;
            }
            if (event.key === 'Home' || event.key === 'End') {
                if (menu.hidden) return;
                event.preventDefault();
                activeIndex = event.key === 'Home' ? 0 : options.length - 1;
                highlight();
                return;
            }
            if (event.key === 'Enter' || event.key === ' ') {
                event.preventDefault();
                if (menu.hidden) open(); else choose(activeIndex);
                return;
            }
            if (event.key === 'Escape' && !menu.hidden) {
                event.preventDefault();
                close();
            }
        });

        control.addEventListener('focus', syncValue);

        menu.addEventListener('mousedown', function (event) {
            // Keep the focus on the control: the menu is driven by aria.
            event.preventDefault();
        });

        menu.addEventListener('click', function (event) {
            var item = event.target.closest('.combobox__option');
            if (!item || item.getAttribute('aria-disabled') === 'true') return;
            choose(Number(item.dataset.index));
        });

        document.addEventListener('click', function (event) {
            if (menu.hidden) return;
            if (wrapper.contains(event.target)) return;
            close();
        });

        window.addEventListener('resize', function () { if (!menu.hidden) place(); });

        select.addEventListener('change', syncValue);

        // Options can be re-rendered by the app (product pickers refill the
        // manufacturer and model lists); keep the label and the width honest.
        if (window.MutationObserver) {
            var observer = new MutationObserver(function () {
                syncValue();
                fitLongestOption();
            });
            observer.observe(select, {
                childList: true,
                subtree: true,
                attributes: true,
                attributeFilter: ['disabled']
            });
        }

        syncValue();
        fitLongestOption();
    }

    document.querySelectorAll('select.form-select').forEach(enhance);
});

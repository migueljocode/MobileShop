// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
window.parseMoneyInput = function (value) {
    const digits = String(value ?? '').replace(/\D/g, '');
    return digits ? Number(digits) : NaN;
};

window.formatMoneyInput = function (input) {
    if (input.type === 'number') {
        input.type = 'text';
        input.inputMode = 'numeric';
    }
    const caret = input.selectionStart;
    const digitsBeforeCaret = caret === null ? null : input.value.slice(0, caret).replace(/\D/g, '').length;
    const digits = input.value.replace(/\D/g, '');
    const formatted = digits.replace(/\B(?=(\d{3})+(?!\d))/g, ' ');

    if (formatted === input.value) return;

    input.value = formatted;
    if (digitsBeforeCaret !== null) {
        let nextCaret = 0;
        let digitCount = 0;
        while (nextCaret < formatted.length && digitCount < digitsBeforeCaret) {
            if (/\d/.test(formatted[nextCaret])) digitCount++;
            nextCaret++;
        }
        input.setSelectionRange(nextCaret, nextCaret);
    }
};

// Keyboard model for the searchable comboboxes (person picker, product picker).
// Defined at the top level of the file on purpose: it must already exist when a
// ready handler runs, so it may not live inside the ready block below.
window.wireComboboxKeys = function (input, list, hooks) {
    if (!input || !list) {
        return { reset: () => { } };
    }

    const openList = hooks && typeof hooks.open === 'function' ? hooks.open : () => { };
    const closeList = hooks && typeof hooks.close === 'function' ? hooks.close : () => { };
    // Optional hook: lets a consumer invalidate a request that is still in flight,
    // so a late response cannot re-open the list the user just dismissed.
    const cancelList = hooks && typeof hooks.cancel === 'function' ? hooks.cancel : () => { };
    let activeIndex = -1;

    const isListOpen = () => !list.classList.contains('d-none');

    const readOptions = () => {
        const options = Array.from(list.querySelectorAll('[role="option"]'));
        const prefix = list.id ? list.id : 'combo';
        options.forEach((option, index) => {
            if (!option.id) option.id = `${prefix}-opt-${index}`;
            option.setAttribute('tabindex', '-1');
        });
        return options;
    };

    const clearActive = () => {
        list.querySelectorAll('[role="option"]').forEach(option => {
            option.classList.remove('is-active');
            option.setAttribute('aria-selected', 'false');
        });
        input.removeAttribute('aria-activedescendant');
        activeIndex = -1;
    };

    const setActive = index => {
        const options = readOptions();
        if (options.length === 0) return;

        const target = Math.max(0, Math.min(index, options.length - 1));
        options.forEach((option, position) => {
            const isActive = position === target;
            option.classList.toggle('is-active', isActive);
            option.setAttribute('aria-selected', isActive ? 'true' : 'false');
        });

        const option = options[target];
        activeIndex = target;
        input.setAttribute('aria-activedescendant', option.id);
        if (typeof option.scrollIntoView === 'function') {
            option.scrollIntoView({ block: 'nearest' });
        }
    };

    input.addEventListener('keydown', event => {
        const key = event.key;
        if (key !== 'ArrowDown' && key !== 'ArrowUp' && key !== 'Enter' && key !== 'Escape') return;

        const options = readOptions();
        const isOpen = isListOpen();

        if (key === 'ArrowDown') {
            if (options.length === 0) return;
            if (!isOpen) {
                openList();
                event.preventDefault();
                return;
            }
            setActive(activeIndex < 0 ? 0 : activeIndex + 1);
            event.preventDefault();
            return;
        }

        if (key === 'ArrowUp') {
            if (!isOpen || options.length === 0) return;
            setActive(activeIndex < 0 ? 0 : activeIndex - 1);
            event.preventDefault();
            return;
        }

        if (key === 'Enter') {
            if (activeIndex < 0) return;
            const option = options[activeIndex];
            if (!option) return;
            event.preventDefault();
            event.stopPropagation();
            option.click();
            return;
        }

        // Escape dismisses the list, so any search that is still in flight is
        // cancelled as well — even when the list is still hidden, because the
        // pending response would otherwise open it a moment later.
        cancelList();
        if (!isOpen) return;
        closeList();
        event.preventDefault();
        event.stopPropagation();
    });

    return { reset: clearActive };
};

const moneyInputSelector =
    '[data-price], [data-amount], [data-finished-price-input], input[inputmode="numeric"][name*="Price"], input[inputmode="numeric"][name*="ProfitAmount"]';

if (typeof $ !== 'undefined' && $.validator) {
    $.validator.setDefaults({
        normalizer: function (value) {
            return this.matches(moneyInputSelector) ? value.replace(/\s/g, '') : value;
        }
    });
}

// Links validation messages and the invalid state to their controls for assistive tech.
const wireValidationAria = () => {
    const messageSelector = '[data-valmsg-for]';
    const controlSelector = 'input, select, textarea';
    const errorClass = 'input-validation-error';

    document.querySelectorAll(messageSelector).forEach(message => {
        message.setAttribute('aria-live', 'polite');

        const target = message.getAttribute('data-valmsg-for');
        if (!target) return;

        if (!message.id) {
            message.id = 'val-' + target.toLowerCase().replace(/[^a-z0-9]/g, '-');
        }

        const control = document.querySelector(`[name="${CSS.escape(target)}"]`);
        if (!control) return;

        const describedBy = (control.getAttribute('aria-describedby') || '')
            .split(/\s+/)
            .filter(token => token.length > 0);
        if (!describedBy.includes(message.id)) describedBy.push(message.id);
        control.setAttribute('aria-describedby', describedBy.join(' '));
    });

    const syncInvalidState = control => {
        if (control.classList.contains(errorClass)) {
            control.setAttribute('aria-invalid', 'true');
        } else {
            control.removeAttribute('aria-invalid');
        }
    };

    const forms = document.querySelectorAll('form');
    forms.forEach(form => {
        form.querySelectorAll(controlSelector).forEach(syncInvalidState);
    });

    const observerCtor = window.MutationObserver;
    if (typeof observerCtor !== 'function') return;

    const observer = new observerCtor(mutations => {
        mutations.forEach(mutation => {
            const target = mutation.target;
            if (target.nodeType === 1 && target.matches(controlSelector)) {
                syncInvalidState(target);
            }
        });
    });

    forms.forEach(form => {
        observer.observe(form, { subtree: true, attributes: true, attributeFilter: ['class'] });
    });
};

document.addEventListener('DOMContentLoaded', () => {
    const moneyInputs = document.querySelectorAll(
        `${moneyInputSelector}, [data-finished-price]`
    );

    moneyInputs.forEach(input => {
        input.type = 'text';
        input.inputMode = 'numeric';
        window.formatMoneyInput(input);
        input.addEventListener('input', () => window.formatMoneyInput(input));
    });

    document.addEventListener('submit', event => {
        moneyInputs.forEach(input => {
            if (input.form === event.target) input.value = input.value.replace(/\s/g, '');
        });
    }, true);

    wireValidationAria();

    // Solid-surfaces switch (Profile page). The choice is remembered per browser in
    // localStorage; the layout applies it before the first paint. Storage may be
    // unavailable or blocked, so every access is guarded and the page still works.
    const surfacesToggle = document.querySelector('[data-surfaces-toggle]');
    if (surfacesToggle) {
        surfacesToggle.checked = document.documentElement.dataset.surfaces === 'solid';

        surfacesToggle.addEventListener('change', () => {
            if (surfacesToggle.checked) {
                document.documentElement.dataset.surfaces = 'solid';
                try {
                    localStorage.setItem('mobileshop.surfaces', 'solid');
                } catch (e) { }
            } else {
                delete document.documentElement.dataset.surfaces;
                try {
                    localStorage.removeItem('mobileshop.surfaces');
                } catch (e) { }
            }
        });
    }

    // Light/dark theme switch. The choice is remembered per browser in
    // localStorage; the layout applies it before the first paint. Storage may be
    // unavailable or blocked, so every access is guarded and the page still works.
    const themeColorMeta = document.querySelector('meta[name="theme-color"]');
    const themeColors = { dark: '#0E1318', light: '#F1F4F7' };
    const applyTheme = theme => {
        document.documentElement.dataset.theme = theme;
        document.documentElement.setAttribute('data-bs-theme', theme);
        if (themeColorMeta) {
            themeColorMeta.setAttribute('content', themeColors[theme] || themeColors.dark);
        }
    };

    const currentTheme = document.documentElement.dataset.theme === 'light' ? 'light' : 'dark';
    applyTheme(currentTheme);

    document.querySelectorAll('[data-theme-toggle]').forEach(toggle => {
        toggle.checked = toggle.value === currentTheme;
        toggle.addEventListener('change', () => {
            if (!toggle.checked) return;
            const theme = toggle.value === 'light' ? 'light' : 'dark';
            applyTheme(theme);
            try {
                localStorage.setItem('mobileshop.theme', theme);
            } catch (e) { }
        });
    });
});

document.addEventListener('click', event => {
    const toggle = event.target.closest('button[data-password-toggle]');
    if (!toggle) return;

    const passwordInput = document.getElementById(toggle.dataset.passwordToggle);
    if (!passwordInput) return;

    const isVisible = passwordInput.type === 'password';
    passwordInput.type = isVisible ? 'text' : 'password';
    toggle.classList.toggle('is-visible', isVisible);
    toggle.setAttribute('aria-pressed', String(isVisible));
    toggle.setAttribute('aria-label', isVisible ? 'Hide password' : 'Show password');
    toggle.title = isVisible ? 'Hide password' : 'Show password';
});

// Enter key form submission - submits the nearest form when Enter is pressed
// in input/select elements, except in textareas and Jalali datepicker inputs
document.addEventListener('keydown', event => {
    if (event.key !== 'Enter') return;
    
    const target = event.target;
    
    // Skip if target is a textarea (Enter should create new lines)
    if (target.tagName === 'TEXTAREA') return;
    
    // Skip if target has contenteditable
    if (target.isContentEditable) return;
    
    // Skip if target is a Jalali datepicker input (has its own handling)
    if (target.closest('[data-jalali-date], [data-jalali-datetime]')) return;
    
    // Skip if target is inside a modal that handles its own Enter
    if (target.closest('.modal')) return;
    
    // Find the closest form
    const form = target.closest('form');
    if (!form) return;
    
    // Find the submit button in the form
    const submitButton = form.querySelector('button[type="submit"], input[type="submit"]');
    if (!submitButton) return;
    
    // Don't submit if the submit button is disabled
    if (submitButton.disabled) return;
    
    // Prevent default (e.g., in select dropdowns)
    event.preventDefault();
    
    // Click the submit button
    submitButton.click();
});

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

    input.removeAttribute('data-val-number');
    input.removeAttribute('data-val-range');
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

document.addEventListener('DOMContentLoaded', () => {
    const moneyInputs = document.querySelectorAll(
        '[data-price], [data-amount], [data-finished-price], [data-finished-price-input], input[inputmode="numeric"][name*="Price"], input[inputmode="numeric"][name*="ProfitAmount"]'
    );

    moneyInputs.forEach(input => {
        input.type = 'text';
        input.inputMode = 'numeric';
        window.formatMoneyInput(input);
        input.addEventListener('input', () => window.formatMoneyInput(input));
    });

    // Add custom jQuery unobtrusive validator for money inputs that parses the formatted value
    if (typeof $ !== 'undefined' && $.validator) {
        $.validator.addMethod('money-range', function (value, element) {
            const numericValue = window.parseMoneyInput(value);
            if (isNaN(numericValue)) return false;
            
            const min = Number($(element).attr('min') || 0);
            const max = Number($(element).attr('max') || Infinity);
            
            return numericValue >= min && numericValue <= max;
        }, 'Please enter a value greater than or equal to {0}.');

        $.validator.unobtrusive.adapters.add('range', ['min', 'max'], function (options) {
            const element = options.element;
            if ($(element).attr('inputmode') === 'numeric' || 
                $(element).is('[data-price], [data-amount], [data-finished-price], [data-finished-price-input]')) {
                options.rules['money-range'] = true;
                delete options.rules['range'];
            }
        });
    }

    document.addEventListener('submit', event => {
        moneyInputs.forEach(input => {
            if (input.form === event.target) input.value = input.value.replace(/\s/g, '');
        });
    }, true);
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

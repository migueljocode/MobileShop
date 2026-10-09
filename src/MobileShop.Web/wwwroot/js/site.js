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

const moneyInputSelector =
    '[data-price], [data-amount], [data-finished-price-input], input[inputmode="numeric"][name*="Price"], input[inputmode="numeric"][name*="ProfitAmount"]';

if (typeof $ !== 'undefined' && $.validator) {
    $.validator.setDefaults({
        normalizer: function (value) {
            return this.matches(moneyInputSelector) ? value.replace(/\s/g, '') : value;
        }
    });
}

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

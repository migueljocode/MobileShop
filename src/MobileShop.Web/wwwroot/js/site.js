// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
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

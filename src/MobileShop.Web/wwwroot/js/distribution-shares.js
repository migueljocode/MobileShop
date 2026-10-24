// Reports > Distribution: opens the saved tab, and keeps the "Edit shares" modal live.
(function () {
    'use strict';

    var bootstrap = window.bootstrap;

    // A redirect after saving lands on #distribution; show that tab.
    if (window.location.hash === '#distribution' && bootstrap) {
        var tab = document.getElementById('distribution-tab');
        if (tab) {
            bootstrap.Tab.getOrCreateInstance(tab).show();
        }
    }

    var modal = document.getElementById('editSharesModal');
    var form = document.getElementById('editSharesForm');
    if (!modal || !form) {
        return;
    }

    var saveButton = form.querySelector('[data-save]');
    var summary = modal.querySelector('[data-summary]');
    var totalProfit = BigInt(modal.dataset.totalProfit || '0');
    var rows = Array.prototype.slice.call(modal.querySelectorAll('[data-share-row]'));
    var shopRow = modal.querySelector('[data-shop-row]');
    var ZERO = BigInt(0);
    var HUNDRED = BigInt(100);

    // Same rounding as the server: floor(profit × percent / 100); no profit means no payout.
    function floorShare(profit, percent) {
        if (profit <= ZERO) {
            return ZERO;
        }
        return profit * BigInt(percent) / HUNDRED;
    }

    // Mirrors ToGroupedDigits + " Toman": spaces between digit groups.
    function formatAmount(value) {
        var negative = value < ZERO;
        var digits = (negative ? -value : value).toString();
        return (negative ? '-' : '') + digits.replace(/\B(?=(\d{3})+(?!\d))/g, ' ') + ' Toman';
    }

    function formatDelta(delta) {
        return (delta > ZERO ? '+' : '') + formatAmount(delta);
    }

    // Returns the whole-number percent, or null when the input is empty, not an integer or outside 0–100.
    function parsePercent(raw) {
        var text = (raw || '').trim();
        if (!/^\d+$/.test(text)) {
            return null;
        }
        var value = parseInt(text, 10);
        return value <= 100 ? value : null;
    }

    function setText(scope, selector, text) {
        var element = scope.querySelector(selector);
        if (element) {
            element.textContent = text;
        }
    }

    function renderRow(row, originalPercent, currentPercent, wasAmount, nowAmount) {
        var changed = currentPercent !== null && currentPercent !== originalPercent;
        row.classList.toggle('is-changed', changed);

        setText(row, '[data-was]', formatAmount(wasAmount));
        setText(row, '[data-now]', nowAmount === null ? '—' : formatAmount(nowAmount));

        var notes = [];
        if (changed) {
            notes.push('was ' + originalPercent + '% → ' + currentPercent + '%');
            if (nowAmount !== null) {
                notes.push(formatDelta(nowAmount - wasAmount));
            }
        }
        setText(row, '[data-change]', notes.join(' · '));
    }

    function recalc() {
        var sum = 0;
        var allValid = true;
        var wasEmployeesTotal = ZERO;
        var nowEmployeesTotal = ZERO;

        rows.forEach(function (row) {
            var input = row.querySelector('[data-share-input]');
            var original = parseInt(row.dataset.originalPercent, 10);
            var percent = parsePercent(input.value);
            var wasAmount = floorShare(totalProfit, original);
            var nowAmount = percent === null ? null : floorShare(totalProfit, percent);

            input.setAttribute('aria-invalid', percent === null ? 'true' : 'false');
            wasEmployeesTotal += wasAmount;
            if (percent === null) {
                allValid = false;
            } else {
                sum += percent;
                nowEmployeesTotal += nowAmount;
            }
            renderRow(row, original, percent, wasAmount, nowAmount);
        });

        var valid = allValid && sum <= 100;
        var shopOriginal = parseInt(shopRow.dataset.originalPercent, 10);
        var shopPercent = valid ? 100 - sum : null;
        var shopWas = totalProfit - wasEmployeesTotal;
        var shopNow = valid ? totalProfit - nowEmployeesTotal : null;
        renderRow(shopRow, shopOriginal, shopPercent, shopWas, shopNow);
        setText(shopRow, '[data-shop-share]', shopPercent === null ? '—' : shopPercent + '%');

        var text;
        if (!allValid) {
            text = 'Enter whole numbers from 0 to 100.';
        } else if (sum > 100) {
            text = 'Over by ' + (sum - 100) + '%';
        } else {
            text = 'Total ' + sum + '% · Shop gets ' + (100 - sum) + '%';
        }
        summary.textContent = text;
        summary.classList.toggle('text-danger', !valid);

        if (saveButton) {
            saveButton.disabled = !valid;
        }
    }

    // Back to the saved values, so every opening starts from the server's numbers.
    function resetValues() {
        rows.forEach(function (row) {
            row.querySelector('[data-share-input]').value = row.dataset.originalPercent;
        });
        recalc();
    }

    modal.addEventListener('input', function (event) {
        if (event.target.matches('[data-share-input]')) {
            recalc();
        }
    });

    var resetButton = modal.querySelector('[data-reset]');
    if (resetButton) {
        resetButton.addEventListener('click', resetValues);
    }

    modal.addEventListener('hidden.bs.modal', resetValues);

    form.addEventListener('submit', function (event) {
        if (saveButton && saveButton.disabled) {
            event.preventDefault();
        }
    });

    recalc();

    // After a rejected save the server re-renders the modal already open with the message.
    if (modal.dataset.openOnLoad === 'true' && bootstrap) {
        bootstrap.Modal.getOrCreateInstance(modal).show();
    }
})();

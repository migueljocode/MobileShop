// Shared pricing behaviour for the Create Phone and Create Apple ID pages.
//
// Keeps the existing percent <-> amount two-way link (editing one updates the other) and shows a
// read-only finished price so the operator can see what the server will store. The server remains
// the source of truth for the persisted finished price (amount-first rule, see
// ProductsDataService.ComputeFinishedPrice); this display-only figure never posts a competing value.
//
// Hooks:
//   [data-price]           paid price input (binds to Input.Price)
//   [data-percent]         profit % input (binds to Input.ProfitPercent)
//   [data-amount]          profit Rial input (binds to Input.ProfitAmount)
//   [data-finished-price]  read-only finished price display element (not posted)
document.addEventListener('DOMContentLoaded', function () {
    const priceEl = document.querySelector('[data-price]');
    const percentEl = document.querySelector('[data-percent]');
    const amountEl = document.querySelector('[data-amount]');
    const finishedEl = document.querySelector('[data-finished-price]');

    if (!priceEl || !percentEl || !amountEl) return;

    function updateFromPercent() {
        const price = parseFloat(priceEl.value);
        const percent = parseFloat(percentEl.value);
        if (isNaN(price) || price === 0 || isNaN(percent)) {
            amountEl.value = '';
            updateFinished();
            return;
        }
        amountEl.value = Math.floor(price * percent / 100).toString();
        updateFinished();
    }

    function updateFromAmount() {
        const price = parseFloat(priceEl.value);
        const amount = parseFloat(amountEl.value);
        if (isNaN(price) || price === 0 || isNaN(amount)) {
            percentEl.value = '';
            updateFinished();
            return;
        }
        percentEl.value = (amount / price * 100).toFixed(2);
        updateFinished();
    }

    // Mirrors the server's finished-price rule: amount wins if present, else percent, else paid.
    function updateFinished() {
        if (!finishedEl) return;

        const price = parseFloat(priceEl.value);
        const amount = parseFloat(amountEl.value);
        const percent = parseFloat(percentEl.value);

        let finished;
        if (!isNaN(amount)) {
            finished = (isNaN(price) ? 0 : price) + amount;
        } else if (!isNaN(percent)) {
            finished = (isNaN(price) ? 0 : price) * (1 + percent / 100);
        } else {
            finished = isNaN(price) ? 0 : price;
        }

        if (finished < 0) finished = 0;

        // Must set .value on <input>; textContent does not update the input display.
        const text = Math.floor(finished).toString();
        if ('value' in finishedEl) {
            finishedEl.value = text;
        } else {
            finishedEl.textContent = text;
        }
    }

    // When paid price changes, re-link %/$ then refresh finished (same live feel as profit fields).
    function onPaidPriceInput() {
        const percent = parseFloat(percentEl.value);
        const amount = parseFloat(amountEl.value);
        if (!isNaN(percent)) {
            updateFromPercent();
        } else if (!isNaN(amount)) {
            updateFromAmount();
        } else {
            updateFinished();
        }
    }

    percentEl.addEventListener('input', updateFromPercent);
    amountEl.addEventListener('input', updateFromAmount);
    priceEl.addEventListener('input', onPaidPriceInput);

    // Populate the read-only figure for any value restored on a validation round-trip.
    updateFinished();
});

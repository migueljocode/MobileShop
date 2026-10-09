// Shamsi (Jalali) date and date-time inputs.
//
// Every input marked with data-jalali-date or data-jalali-datetime shows and accepts Shamsi
// values (1405/07/18 or 1405/07/18 14:30) and gets a popup Shamsi calendar. The page posts the
// Shamsi text as typed; the server keeps storing Gregorian values.
//
// The Jalali <-> Gregorian conversion is the standard jalaali-js algorithm by Behrang Noruzi Niya
// (MIT licensed). Wiring is delegated from the document so it also covers markup injected later
// (the create-product modal loads its form over fetch).
(function () {
    'use strict';

    var BREAKS = [-61, 9, 38, 199, 426, 686, 756, 818, 1111, 1181, 1210, 1635, 2060, 2097, 2192, 2262, 2324, 2394, 2456, 3178];
    var MONTH_NAMES = ['Farvardin', 'Ordibehesht', 'Khordad', 'Tir', 'Mordad', 'Shahrivar', 'Mehr', 'Aban', 'Azar', 'Dey', 'Bahman', 'Esfand'];
    var WEEKDAY_NAMES = ['ش', 'ی', 'د', 'س', 'چ', 'پ', 'ج'];
    var MIN_YEAR = 1200;
    var MAX_YEAR = 1600;
    var INPUT_SELECTOR = '[data-jalali-date], [data-jalali-datetime]';
    var INVALID_MESSAGE = 'Enter a Shamsi date such as 1405/07/18.';
    var SHAMSI_PATTERN = /^\s*(\d{4})\s*[/\-.]\s*(\d{1,2})\s*[/\-.]\s*(\d{1,2})(?:[T\s]+(\d{1,2})\s*:\s*(\d{1,2})(?:\s*:\s*(\d{1,2}))?)?\s*$/;

    function div(a, b) { return ~~(a / b); }
    function mod(a, b) { return a - ~~(a / b) * b; }

    function jalCal(jy, withoutLeap) {
        var bl = BREAKS.length;
        var gy = jy + 621;
        var leapJ = -14;
        var jp = BREAKS[0];
        var jm, jump, leap, leapG, march, n, i;

        if (jy < jp || jy >= BREAKS[bl - 1]) throw new Error('Invalid Jalaali year ' + jy);

        for (i = 1; i < bl; i += 1) {
            jm = BREAKS[i];
            jump = jm - jp;
            if (jy < jm) break;
            leapJ = leapJ + div(jump, 33) * 8 + div(mod(jump, 33), 4);
            jp = jm;
        }
        n = jy - jp;

        leapJ = leapJ + div(n, 33) * 8 + div(mod(n, 33) + 3, 4);
        if (mod(jump, 33) === 4 && jump - n === 4) leapJ += 1;
        leapG = div(gy, 4) - div((div(gy, 100) + 1) * 3, 4) - 150;
        march = 20 + leapJ - leapG;

        if (!withoutLeap) {
            if (jump - n < 6) n = n - jump + div(jump + 4, 33) * 33;
            leap = mod(mod(n + 1, 33) - 1, 4);
            if (leap === -1) leap = 4;
        }

        return { leap: leap, gy: gy, march: march };
    }

    function g2d(gy, gm, gd) {
        var d = div((gy + div(gm - 8, 6) + 100100) * 1461, 4) + div(153 * mod(gm + 9, 12) + 2, 5) + gd - 34840408;
        d = d - div(div(gy + 100100 + div(gm - 8, 6), 100) * 3, 4) + 752;
        return d;
    }

    function d2g(jdn) {
        var j = 4 * jdn + 139361631;
        j = j + div(div(4 * jdn + 183187720, 146097) * 3, 4) * 4 - 3908;
        var i = div(mod(j, 1461), 4) * 5 + 308;
        var gd = div(mod(i, 153), 5) + 1;
        var gm = mod(div(i, 153), 12) + 1;
        var gy = div(j, 1461) - 100100 + div(8 - gm, 6);
        return { gy: gy, gm: gm, gd: gd };
    }

    function j2d(jy, jm, jd) {
        var r = jalCal(jy, true);
        return g2d(r.gy, 3, r.march) + (jm - 1) * 31 - div(jm, 7) * (jm - 7) + jd - 1;
    }

    function d2j(jdn) {
        var gy = d2g(jdn).gy;
        var jy = gy - 621;
        var r = jalCal(jy, false);
        var jdn1f = g2d(gy, 3, r.march);
        var jd, jm;
        var k = jdn - jdn1f;

        if (k >= 0) {
            if (k <= 185) {
                jm = 1 + div(k, 31);
                jd = mod(k, 31) + 1;
                return { jy: jy, jm: jm, jd: jd };
            }
            k -= 186;
        } else {
            jy -= 1;
            k += 179;
            if (r.leap === 1) k += 1;
        }

        jm = 7 + div(k, 30);
        jd = mod(k, 30) + 1;
        return { jy: jy, jm: jm, jd: jd };
    }

    function isLeapJalaliYear(jy) { return jalCal(jy, false).leap === 0; }

    function jalaliMonthLength(jy, jm) {
        if (jm <= 6) return 31;
        if (jm <= 11) return 30;
        return isLeapJalaliYear(jy) ? 30 : 29;
    }

    function toJalali(date) { return d2j(g2d(date.getFullYear(), date.getMonth() + 1, date.getDate())); }

    function toGregorian(jy, jm, jd) {
        var gregorian = d2g(j2d(jy, jm, jd));
        return new Date(gregorian.gy, gregorian.gm - 1, gregorian.gd);
    }

    function pad2(value) { return (value < 10 ? '0' : '') + value; }

    function todayShamsi() { return toJalali(new Date()); }

    function parseShamsiText(text) {
        var match = SHAMSI_PATTERN.exec(text || '');
        if (!match) return null;

        var jy = Number(match[1]);
        var jm = Number(match[2]);
        var jd = Number(match[3]);
        if (jy < MIN_YEAR || jy > MAX_YEAR || jm < 1 || jm > 12 || jd < 1 || jd > jalaliMonthLength(jy, jm)) return null;

        var hasTime = match[4] !== undefined;
        var hour = hasTime ? Number(match[4]) : 0;
        var minute = hasTime ? Number(match[5]) : 0;
        var second = match[6] === undefined ? 0 : Number(match[6]);
        if (hour > 23 || minute > 59 || second > 59) return null;

        return { jy: jy, jm: jm, jd: jd, hour: hour, minute: minute, second: second, hasTime: hasTime };
    }

    function formatShamsiText(parsed, withTime) {
        var text = parsed.jy + '/' + pad2(parsed.jm) + '/' + pad2(parsed.jd);
        if (withTime) text += ' ' + pad2(parsed.hour) + ':' + pad2(parsed.minute);
        return text;
    }

    window.MobileShopJalali = {
        parse: parseShamsiText,
        format: formatShamsiText,
        today: todayShamsi,
        toGregorian: toGregorian,
        toJalali: toJalali,
        monthLength: jalaliMonthLength
    };

    // ── Popup calendar ───────────────────────────────────────────────────────
    var ui = null;
    var state = null;

    function buildPanel() {
        var panel = document.createElement('div');
        panel.className = 'jalali-picker-panel';
        panel.setAttribute('role', 'dialog');
        panel.setAttribute('aria-label', 'Shamsi calendar');
        panel.hidden = true;
        panel.innerHTML =
            '<div class="jalali-picker-head">' +
                '<button type="button" class="jalali-picker-nav" data-nav="-1" aria-label="Previous month">&lsaquo;</button>' +
                '<span class="jalali-picker-title" data-title></span>' +
                '<button type="button" class="jalali-picker-nav" data-nav="1" aria-label="Next month">&rsaquo;</button>' +
            '</div>' +
            '<div class="jalali-picker-weekdays">' +
                WEEKDAY_NAMES.map(function (name) { return '<span>' + name + '</span>'; }).join('') +
            '</div>' +
            '<div class="jalali-picker-grid" data-grid></div>' +
            '<div class="jalali-picker-time" data-time hidden>' +
                '<span class="jalali-picker-time-label">Time</span>' +
                '<input type="number" min="0" max="23" data-hour aria-label="Hour" />' +
                '<span class="jalali-picker-time-separator">:</span>' +
                '<input type="number" min="0" max="59" data-minute aria-label="Minute" />' +
                '<button type="button" class="jalali-picker-action" data-now>Now</button>' +
            '</div>' +
            '<div class="jalali-picker-footer">' +
                '<button type="button" class="jalali-picker-action" data-today>Today</button>' +
                '<button type="button" class="jalali-picker-action" data-clear>Clear</button>' +
            '</div>';
        document.body.appendChild(panel);

        var built = {
            panel: panel,
            title: panel.querySelector('[data-title]'),
            grid: panel.querySelector('[data-grid]'),
            time: panel.querySelector('[data-time]'),
            hour: panel.querySelector('[data-hour]'),
            minute: panel.querySelector('[data-minute]')
        };

        panel.querySelectorAll('[data-nav]').forEach(function (button) {
            button.addEventListener('click', function () {
                var step = Number(button.getAttribute('data-nav'));
                state.jm += step;
                if (state.jm < 1) { state.jm = 12; state.jy -= 1; }
                if (state.jm > 12) { state.jm = 1; state.jy += 1; }
                render();
            });
        });

        panel.querySelector('[data-today]').addEventListener('click', function () {
            var now = new Date();
            var today = toJalali(now);
            state.jy = today.jy;
            state.jm = today.jm;
            setTime(now.getHours(), now.getMinutes());
            writeValue(today.jy, today.jm, today.jd);
            if (!state.withTime) render();
        });

        panel.querySelector('[data-now]').addEventListener('click', function () {
            var now = new Date();
            setTime(now.getHours(), now.getMinutes());
            var parsed = parseShamsiText(state.input.value) || toJalali(now);
            writeValue(parsed.jy, parsed.jm, parsed.jd);
        });

        panel.querySelector('[data-clear]').addEventListener('click', function () {
            state.input.value = '';
            markValid(state.input);
            close();
        });

        function onTimeChanged() {
            var hour = Number(built.hour.value);
            var minute = Number(built.minute.value);
            if (isNaN(hour) || isNaN(minute)) return;
            setTime(hour, minute);
            var parsed = parseShamsiText(state.input.value);
            if (parsed) writeValue(parsed.jy, parsed.jm, parsed.jd);
        }

        built.hour.addEventListener('change', onTimeChanged);
        built.minute.addEventListener('change', onTimeChanged);

        // Keeping the focus on the input prevents the blur handler from closing the panel
        // before a day click is delivered.
        panel.addEventListener('mousedown', function (event) { event.preventDefault(); });

        ui = built;
    }

    function setTime(hour, minute) {
        state.hour = Math.min(23, Math.max(0, hour));
        state.minute = Math.min(59, Math.max(0, minute));
        ui.hour.value = String(state.hour);
        ui.minute.value = String(state.minute);
    }

    function render() {
        ui.title.textContent = MONTH_NAMES[state.jm - 1] + ' ' + state.jy;
        ui.grid.replaceChildren();

        var offset = (toGregorian(state.jy, state.jm, 1).getDay() + 1) % 7;
        for (var blank = 0; blank < offset; blank += 1) {
            ui.grid.appendChild(document.createElement('span'));
        }

        var today = todayShamsi();
        var selected = parseShamsiText(state.input.value);
        var length = jalaliMonthLength(state.jy, state.jm);

        for (var day = 1; day <= length; day += 1) {
            var button = document.createElement('button');
            button.type = 'button';
            button.className = 'jalali-picker-day';
            button.textContent = String(day);
            if (state.jy === today.jy && state.jm === today.jm && day === today.jd) button.classList.add('is-today');
            if (selected && selected.jy === state.jy && selected.jm === state.jm && selected.jd === day) button.classList.add('is-selected');
            button.addEventListener('click', pickDay.bind(null, day));
            ui.grid.appendChild(button);
        }
    }

    function pickDay(day) {
        writeValue(state.jy, state.jm, day);
        if (!state.withTime) close();
    }

    function writeValue(jy, jm, jd) {
        state.input.value = formatShamsiText(
            { jy: jy, jm: jm, jd: jd, hour: state.hour, minute: state.minute },
            state.withTime);
        markValid(state.input);
        state.input.dispatchEvent(new Event('change', { bubbles: true }));
        close();
    }

    function position() {
        var rect = state.input.getBoundingClientRect();
        var panel = ui.panel;
        panel.style.minWidth = Math.max(rect.width, 240) + 'px';

        var left = Math.min(rect.left, window.innerWidth - panel.offsetWidth - 8);
        panel.style.left = Math.max(8, left) + 'px';

        var top = rect.bottom + 6;
        if (top + panel.offsetHeight > window.innerHeight - 8) {
            top = Math.max(8, rect.top - panel.offsetHeight - 6);
        }
        panel.style.top = top + 'px';
    }

    function isOpenFor(input) { return Boolean(state && ui && !ui.panel.hidden && state.input === input); }

    function open(input) {
        if (!ui) buildPanel();

        state = {
            input: input,
            withTime: input.hasAttribute('data-jalali-datetime'),
            jy: 0,
            jm: 0,
            hour: 0,
            minute: 0
        };

        var parsed = parseShamsiText(input.value);
        var now = new Date();
        state.jy = parsed ? parsed.jy : todayShamsi().jy;
        state.jm = parsed ? parsed.jm : todayShamsi().jm;
        setTime(parsed && parsed.hasTime ? parsed.hour : now.getHours(),
                 parsed && parsed.hasTime ? parsed.minute : now.getMinutes());

        ui.time.hidden = !state.withTime;
        ui.panel.hidden = false;
        render();
        position();
    }

    function close() {
        if (!ui) return;
        ui.panel.hidden = true;
        state = null;
    }

    // ── Wiring ───────────────────────────────────────────────────────────────
    function feedbackElement(input) {
        var next = input.nextElementSibling;
        if (next && next.classList.contains('jalali-picker-feedback')) return next;

        var feedback = document.createElement('div');
        feedback.className = 'invalid-feedback jalali-picker-feedback';
        input.insertAdjacentElement('afterend', feedback);
        return feedback;
    }

    function markInvalid(input, message) {
        input.classList.add('is-invalid');
        feedbackElement(input).textContent = message;
    }

    function markValid(input) {
        input.classList.remove('is-invalid');
        var next = input.nextElementSibling;
        if (next && next.classList.contains('jalali-picker-feedback')) next.textContent = '';
    }

    function normalize(input) {
        var value = input.value.trim();
        if (!value) { markValid(input); return true; }

        var parsed = parseShamsiText(value);
        if (!parsed) { markInvalid(input, INVALID_MESSAGE); return false; }

        markValid(input);
        input.value = formatShamsiText(parsed, input.hasAttribute('data-jalali-datetime'));
        return true;
    }

    function wire(input) {
        if (input.dataset.jalaliWired === 'true') return;
        input.dataset.jalaliWired = 'true';
        input.setAttribute('autocomplete', 'off');

        input.addEventListener('focus', function () { open(input); });
        input.addEventListener('click', function () { if (!isOpenFor(input)) open(input); });
        input.addEventListener('keydown', function (event) { if (event.key === 'Escape') close(); });
        input.addEventListener('input', function () {
            if (parseShamsiText(input.value)) markValid(input);
        });
        input.addEventListener('blur', function () { normalize(input); });
    }

    document.addEventListener('focusin', function (event) {
        var input = event.target.closest ? event.target.closest(INPUT_SELECTOR) : null;
        if (input) wire(input);
    });

    document.addEventListener('mousedown', function (event) {
        if (!state) return;
        if (ui.panel.contains(event.target) || event.target === state.input) return;
        close();
    });

    document.addEventListener('keydown', function (event) {
        if (event.key === 'Escape') close();
    });

    document.addEventListener('submit', function (event) {
        var invalid = null;
        event.target.querySelectorAll(INPUT_SELECTOR).forEach(function (input) {
            if (!normalize(input) && !invalid) invalid = input;
        });

        if (invalid) {
            event.preventDefault();
            invalid.focus();
        }
    }, true);

    window.addEventListener('scroll', function () { if (state) position(); }, true);
    window.addEventListener('resize', function () { if (state) position(); });
})();

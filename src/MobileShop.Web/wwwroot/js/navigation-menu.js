document.addEventListener('DOMContentLoaded', function () {
    var navigation = document.querySelector('[data-floating-navigation]');
    if (!navigation) return;

    var flyout = navigation.querySelector('[data-navigation-flyout]');
    var dock = navigation.querySelector('[data-navigation-dock]');
    var tabs = navigation.querySelector('.navigation-dock__tabs');
    var triggers = Array.from(navigation.querySelectorAll('[data-navigation-trigger]'));
    var panels = Array.from(navigation.querySelectorAll('[data-navigation-panel]'));
    if (!flyout || !dock || !tabs || triggers.length === 0) return;

    var openedId = null;
    var closeTimer;
    var closeCleanupTimer;
    var gapCloseTimer;
    var closeDelay = 220;
    var suppressFocusOpen = false;
    var suppressInitialHover = navigation.matches(':hover');
    var panelAnimationTimers = new WeakMap();

    var reduceMotion = window.matchMedia('(prefers-reduced-motion: reduce)');
    var finePointer = window.matchMedia('(hover: hover) and (pointer: fine)');
    var compactTimer;
    var scrollTimer;
    var settleTimer;
    var scrolling = false;
    var lastX = 0;
    var lastY = 0;

    function wantsCompact() {
        if (reduceMotion.matches || openedId !== null || navigation.classList.contains('is-engaged')) return false;
        if (navigation.contains(document.activeElement)) return false;
        if (scrolling) return true;
        return finePointer.matches && !navigation.matches(':hover');
    }

    function applyCompact() {
        var compact = wantsCompact();
        if (compact === navigation.classList.contains('is-compact')) return;
        navigation.classList.toggle('is-compact', compact);
        if (compact) {
            window.clearTimeout(settleTimer);
            navigation.classList.remove('is-settling');
            return;
        }
        navigation.classList.add('is-settling');
        window.clearTimeout(settleTimer);
        settleTimer = window.setTimeout(function () {
            navigation.classList.remove('is-settling');
            var element = document.elementFromPoint(lastX, lastY);
            var trigger = element && element.closest('[data-navigation-trigger]');
            if (trigger && navigation.matches(':hover')) open(trigger.dataset.navigationTrigger);
        }, 190);
    }

    function scheduleCompact(delay) {
        window.clearTimeout(compactTimer);
        compactTimer = window.setTimeout(applyCompact, delay);
    }

    function setExpanded(id) {
        triggers.forEach(function (trigger) {
            var expanded = trigger.dataset.navigationTrigger === id;
            trigger.setAttribute('aria-expanded', expanded ? 'true' : 'false');
            trigger.classList.toggle('is-expanded', expanded);
        });
    }

    function showPanel(id) {
        var panel = panels.find(function (candidate) {
            return candidate.dataset.navigationPanel === id;
        });
        if (!panel) return;

        var currentPanel = panels.find(function (candidate) {
            return !candidate.hidden;
        });
        if (currentPanel === panel) {
            clearPanelCloseState(panel);
            return;
        }

        var previousAnimationTimer = panelAnimationTimers.get(panel);
        if (previousAnimationTimer) window.clearTimeout(previousAnimationTimer);
        panels.forEach(function (candidate) {
            candidate.hidden = candidate !== panel;
            candidate.classList.remove('is-switching');
            candidate.classList.remove('is-entering');
            clearPanelCloseState(candidate);
        });
        panel.classList.add('is-switching');

        window.requestAnimationFrame(function () {
            window.requestAnimationFrame(function () {
                panel.classList.remove('is-switching');
                panel.classList.add('is-entering');
                var animationTimer = window.setTimeout(function () {
                    panel.classList.remove('is-entering');
                    panelAnimationTimers.delete(panel);
                }, 800);
                panelAnimationTimers.set(panel, animationTimer);
            });
        });
    }

    function clearPanelCloseState(panel) {
        panel.classList.remove('is-closing');
    }

    function open(id) {
        window.clearTimeout(closeTimer);
        window.clearTimeout(closeCleanupTimer);
        flyout.inert = false;
        flyout.classList.remove('is-closing');
        openedId = id;
        navigation.classList.add('is-engaged');
        applyCompact();
        setExpanded(id);
        showPanel(id);
        flyout.setAttribute('aria-hidden', 'false');
        flyout.classList.add('is-open');
    }

    function scheduleClose() {
        window.clearTimeout(closeTimer);
        closeTimer = window.setTimeout(function () {
            if (isWithinNavigation()) return;
            close();
        }, closeDelay);
    }

    function isWithinNavigation() {
        return navigation.matches(':hover') || navigation.contains(document.activeElement);
    }

    function close() {
        if (openedId === null) return;
        openedId = null;
        var visiblePanel = panels.find(function (panel) {
            return !panel.hidden;
        });
        if (visiblePanel) {
            visiblePanel.classList.remove('is-entering', 'is-switching');
            visiblePanel.classList.add('is-closing');
        }
        setExpanded(null);
        flyout.setAttribute('aria-hidden', 'true');
        flyout.inert = true;
        flyout.classList.add('is-closing');
        flyout.classList.remove('is-open');
        window.clearTimeout(closeCleanupTimer);
        closeCleanupTimer = window.setTimeout(function () {
            finishClose();
        }, 340);
    }

    triggers.forEach(function (trigger, index) {
        trigger.addEventListener('mouseenter', function () {
            if (suppressInitialHover || navigation.classList.contains('is-settling')) return;
            var id = trigger.dataset.navigationTrigger;
            open(id);
        });

        trigger.addEventListener('focus', function () {
            if (suppressFocusOpen) return;
            open(trigger.dataset.navigationTrigger);
        });

        trigger.addEventListener('keydown', function (event) {
            var nextIndex;
            if (event.key === 'ArrowRight' || event.key === 'ArrowDown') nextIndex = (index + 1) % triggers.length;
            else if (event.key === 'ArrowLeft' || event.key === 'ArrowUp') nextIndex = (index - 1 + triggers.length) % triggers.length;
            else if (event.key === 'Home') nextIndex = 0;
            else if (event.key === 'End') nextIndex = triggers.length - 1;
            else return;

            event.preventDefault();
            triggers[nextIndex].focus();
        });
    });

    tabs.addEventListener('pointermove', function (event) {
        if (event.pointerType !== 'mouse' || openedId === null) return;
        if (event.target.closest('[data-navigation-trigger]')) {
            window.clearTimeout(gapCloseTimer);
            return;
        }

        window.clearTimeout(gapCloseTimer);
        gapCloseTimer = window.setTimeout(function () {
            if (!tabs.matches(':hover') || flyout.matches(':hover')) return;
            close();
        }, 100);
    });

    flyout.addEventListener('mouseenter', function () {
        window.clearTimeout(closeTimer);
        window.clearTimeout(gapCloseTimer);
    });

    /* A focused tile grows and glows; scroll it clear of the list's soft edges
       so the ring is never sliced by the viewport. */
    flyout.addEventListener('focusin', function (event) {
        var link = event.target.closest ? event.target.closest('.navigation-link') : null;
        if (!link) return;
        link.scrollIntoView({ block: 'nearest', inline: 'nearest' });
    });

    function finishClose() {
        if (openedId !== null) return;
        window.clearTimeout(closeCleanupTimer);
        flyout.classList.remove('is-closing');
        panels.forEach(function (panel) {
            clearPanelCloseState(panel);
        });
        navigation.classList.remove('is-engaged');
        scheduleCompact(160);
    }

    flyout.addEventListener('transitionend', function (event) {
        if (event.target === flyout && event.propertyName === 'max-height' && openedId === null)
            finishClose();
    });
    navigation.addEventListener('pointerenter', function () {
        window.clearTimeout(closeTimer);
        window.clearTimeout(compactTimer);
        applyCompact();
    });
    navigation.addEventListener('pointerleave', function (event) {
        if (event.relatedTarget && navigation.contains(event.relatedTarget)) return;
        suppressInitialHover = false;
        navigation.classList.remove('is-initializing');
        scheduleClose();
        scheduleCompact(160);
    });
    navigation.addEventListener('focusout', function (event) {
        if (!navigation.contains(event.relatedTarget)) { scheduleClose(); scheduleCompact(160); }
    });

    navigation.addEventListener('keydown', function (event) {
        if (event.key === 'Escape' && openedId !== null) {
            var activeTrigger = triggers.find(function (trigger) {
                return trigger.dataset.navigationTrigger === openedId;
            });
            close();
            if (activeTrigger) {
                suppressFocusOpen = true;
                activeTrigger.focus();
                window.requestAnimationFrame(function () {
                    suppressFocusOpen = false;
                });
            }
        }
    });

    navigation.addEventListener('focusin', applyCompact);
    navigation.addEventListener('pointermove', function (event) {
        lastX = event.clientX;
        lastY = event.clientY;
        if (navigation.classList.contains('is-compact')) applyCompact();
    }, { passive: true });
    window.addEventListener('scroll', function () {
        scrolling = true;
        applyCompact();
        window.clearTimeout(scrollTimer);
        scrollTimer = window.setTimeout(function () {
            scrolling = false;
            applyCompact();
        }, 450);
    }, { passive: true });
    if (reduceMotion.addEventListener) reduceMotion.addEventListener('change', applyCompact);
    document.addEventListener('pointerdown', function (event) {
        if (openedId !== null && !navigation.contains(event.target)) close();
    });

    applyCompact();
    window.requestAnimationFrame(function () {
        window.requestAnimationFrame(function () {
            navigation.classList.remove('is-initializing');
        });
    });
});

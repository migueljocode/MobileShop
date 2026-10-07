document.addEventListener('DOMContentLoaded', function () {
    var navigation = document.querySelector('[data-floating-navigation]');
    if (!navigation) return;

    var flyout = navigation.querySelector('[data-navigation-flyout]');
    var dock = navigation.querySelector('[data-navigation-dock]');
    var triggers = Array.from(navigation.querySelectorAll('[data-navigation-trigger]'));
    var panels = Array.from(navigation.querySelectorAll('[data-navigation-panel]'));
    if (!flyout || !dock || triggers.length === 0) return;

    var openedId = null;
    var closeTimer;
    var closeCleanupTimer;
    var closeDelay = 220;
    var suppressFocusOpen = false;
    var suppressInitialHover = navigation.matches(':hover');
    var panelAnimationTimers = new WeakMap();

    if (!suppressInitialHover) navigation.classList.remove('is-initializing');

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
                }, 650);
                panelAnimationTimers.set(panel, animationTimer);
            });
        });
    }

    function clearPanelCloseState(panel) {
        panel.classList.remove('is-closing');
        panel.style.removeProperty('--collapse-delay');
        panel.querySelectorAll('.navigation-link').forEach(function (link) {
            link.style.removeProperty('--collapse-delay');
        });
    }

    function open(id) {
        window.clearTimeout(closeTimer);
        window.clearTimeout(closeCleanupTimer);
        flyout.inert = false;
        flyout.classList.remove('is-closing');
        var wasClosed = openedId === null;
        openedId = id;
        navigation.classList.add('is-engaged');
        setExpanded(id);
        showPanel(id);
        flyout.setAttribute('aria-hidden', 'false');
        if (wasClosed) dock.classList.add('is-opening');
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
            visiblePanel.querySelectorAll('.navigation-link').forEach(function (link, index, links) {
                link.style.setProperty('--collapse-delay', (35 + (links.length - index - 1) * 32) + 'ms');
            });
        }
        setExpanded(null);
        flyout.setAttribute('aria-hidden', 'true');
        flyout.inert = true;
        flyout.classList.add('is-closing');
        flyout.classList.remove('is-open');
        dock.classList.remove('is-opening');
        window.clearTimeout(closeCleanupTimer);
        closeCleanupTimer = window.setTimeout(function () {
            finishClose();
        }, 480);
    }

    triggers.forEach(function (trigger, index) {
        trigger.addEventListener('mouseenter', function () {
            if (suppressInitialHover) return;
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

    flyout.addEventListener('mouseenter', function () {
        window.clearTimeout(closeTimer);
    });
    function finishClose() {
        if (openedId !== null) return;
        window.clearTimeout(closeCleanupTimer);
        flyout.classList.remove('is-closing');
        panels.forEach(function (panel) {
            clearPanelCloseState(panel);
        });
        navigation.classList.remove('is-engaged');
    }

    flyout.addEventListener('transitionend', function (event) {
        if (event.target === flyout && event.propertyName === 'max-height' && openedId === null)
            finishClose();
    });
    dock.addEventListener('animationend', function (event) {
        if (event.target === dock && event.animationName === 'navigation-shell-open')
            dock.classList.remove('is-opening');
    });

    navigation.addEventListener('pointerenter', function () {
        window.clearTimeout(closeTimer);
    });
    navigation.addEventListener('pointerleave', function (event) {
        if (event.relatedTarget && navigation.contains(event.relatedTarget)) return;
        suppressInitialHover = false;
        navigation.classList.remove('is-initializing');
        scheduleClose();
    });
    navigation.addEventListener('focusout', function (event) {
        if (!navigation.contains(event.relatedTarget)) scheduleClose();
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

    document.addEventListener('pointerdown', function (event) {
        if (openedId !== null && !navigation.contains(event.target)) close();
    });

});

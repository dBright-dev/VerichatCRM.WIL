// Landing page behaviour: nav state, mobile menu, scroll reveal, the workflow
// tabs, the catalogue lightbox and the pointer highlight on cards.
// Everything degrades: with JS off the page is complete and readable, because the
// reveal styles and tab panels are only hidden once .lnd-js is on <html>.
(function () {
    'use strict';

    var root = document.documentElement;
    root.classList.add('lnd-js');

    var reduceMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;

    /* ---------- navigation ---------- */
    var nav = document.getElementById('lndNav');
    var links = document.getElementById('lndNavLinks');
    var toggle = document.querySelector('.lnd-nav__toggle');

    function setStuck() {
        if (!nav) return;
        nav.classList.toggle('is-stuck', window.scrollY > 24);
    }
    setStuck();

    var ticking = false;
    window.addEventListener('scroll', function () {
        if (ticking) return;
        ticking = true;
        window.requestAnimationFrame(function () { setStuck(); ticking = false; });
    }, { passive: true });

    function closeMenu() {
        if (!links || !toggle) return;
        links.classList.remove('is-open');
        toggle.setAttribute('aria-expanded', 'false');
    }

    if (toggle && links) {
        toggle.addEventListener('click', function () {
            var open = links.classList.toggle('is-open');
            toggle.setAttribute('aria-expanded', open ? 'true' : 'false');
        });
        links.addEventListener('click', function (e) {
            if (e.target.closest('a')) closeMenu();
        });
    }

    /* ---------- reveal on scroll ---------- */
    var revealables = Array.prototype.slice.call(document.querySelectorAll('[data-reveal]'));
    revealables.forEach(function (el) {
        var d = el.getAttribute('data-reveal-delay');
        if (d) el.style.setProperty('--lnd-delay', d + 'ms');
    });

    if (reduceMotion || !('IntersectionObserver' in window)) {
        revealables.forEach(function (el) { el.classList.add('is-in'); });
    } else {
        var revealer = new IntersectionObserver(function (entries) {
            entries.forEach(function (entry) {
                if (!entry.isIntersecting) return;
                entry.target.classList.add('is-in');
                revealer.unobserve(entry.target);
            });
        }, { rootMargin: '0px 0px -12% 0px', threshold: 0.12 });
        revealables.forEach(function (el) { revealer.observe(el); });
    }

    /* ---------- which section is on screen ---------- */
    var navLinks = Array.prototype.slice.call(document.querySelectorAll('.lnd-nav__links a[href^="#"]'));
    var sections = navLinks
        .map(function (a) { return document.querySelector(a.getAttribute('href')); })
        .filter(Boolean);

    if (sections.length && 'IntersectionObserver' in window) {
        var spy = new IntersectionObserver(function (entries) {
            entries.forEach(function (entry) {
                if (!entry.isIntersecting) return;
                navLinks.forEach(function (a) {
                    a.classList.toggle('is-active', a.getAttribute('href') === '#' + entry.target.id);
                });
            });
        }, { rootMargin: '-45% 0px -50% 0px' });
        sections.forEach(function (s) { spy.observe(s); });
    }

    /* ---------- workflow tabs ---------- */
    var tabList = document.querySelector('[data-tabs]');
    if (tabList) {
        var tabs = Array.prototype.slice.call(tabList.querySelectorAll('[role="tab"]'));
        var panels = tabs.map(function (t) { return document.getElementById(t.getAttribute('aria-controls')); });

        function select(i, moveFocus) {
            tabs.forEach(function (tab, n) {
                var on = n === i;
                tab.setAttribute('aria-selected', on ? 'true' : 'false');
                tab.setAttribute('tabindex', on ? '0' : '-1');
                if (panels[n]) {
                    panels[n].hidden = !on;
                    if (on && !reduceMotion) {
                        panels[n].classList.remove('lnd-l-fade');
                        void panels[n].offsetWidth;
                        panels[n].classList.add('lnd-l-fade');
                    }
                }
            });
            if (moveFocus) tabs[i].focus();
        }

        tabs.forEach(function (tab, i) {
            tab.addEventListener('click', function () { select(i, false); });
            tab.addEventListener('keydown', function (e) {
                var last = tabs.length - 1;
                if (e.key === 'ArrowRight' || e.key === 'ArrowDown') { select(i === last ? 0 : i + 1, true); e.preventDefault(); }
                else if (e.key === 'ArrowLeft' || e.key === 'ArrowUp') { select(i === 0 ? last : i - 1, true); e.preventDefault(); }
                else if (e.key === 'Home') { select(0, true); e.preventDefault(); }
                else if (e.key === 'End') { select(last, true); e.preventDefault(); }
            });
        });
    }

    /* ---------- pointer highlight on catalogue cards ---------- */
    document.querySelectorAll('.lnd-card').forEach(function (card) {
        card.addEventListener('pointermove', function (e) {
            var r = card.getBoundingClientRect();
            card.style.setProperty('--mx', (e.clientX - r.left) + 'px');
            card.style.setProperty('--my', (e.clientY - r.top) + 'px');
        });
    });

    /* ---------- lightbox ---------- */
    var box = document.getElementById('lndLightbox');
    if (box) {
        var img = box.querySelector('.lnd-lightbox__img');
        var nameEl = box.querySelector('.lnd-lightbox__cap b');
        var detailEl = box.querySelector('.lnd-lightbox__detail');
        var closeBtn = box.querySelector('.lnd-lightbox__close');
        var lastFocus = null;

        function open(trigger) {
            lastFocus = trigger;
            img.src = trigger.getAttribute('data-full');
            img.alt = trigger.getAttribute('data-alt') || '';
            nameEl.textContent = trigger.getAttribute('data-name') || '';
            detailEl.textContent = trigger.getAttribute('data-detail') || '';
            box.classList.add('is-open');
            box.removeAttribute('hidden');
            document.body.style.overflow = 'hidden';
            closeBtn.focus();
        }

        function close() {
            box.classList.remove('is-open', 'is-zoomed');
            document.body.style.overflow = '';
            window.setTimeout(function () { box.setAttribute('hidden', ''); }, 350);
            if (lastFocus) lastFocus.focus();
        }

        document.querySelectorAll('[data-lightbox]').forEach(function (trigger) {
            trigger.addEventListener('click', function () { open(trigger); });
        });

        closeBtn.addEventListener('click', close);
        box.addEventListener('click', function (e) { if (e.target === box) close(); });
        document.addEventListener('keydown', function (e) {
            if (e.key === 'Escape' && box.classList.contains('is-open')) close();
        });

        img.addEventListener('click', function (e) {
            var r = img.getBoundingClientRect();
            img.style.transformOrigin = ((e.clientX - r.left) / r.width * 100) + '% ' + ((e.clientY - r.top) / r.height * 100) + '%';
            box.classList.toggle('is-zoomed');
        });
    }
})();

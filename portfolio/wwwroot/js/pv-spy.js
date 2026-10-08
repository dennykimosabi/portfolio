/* Workshop scrollspy: highlights the component-list anchor for the section in view. */
window.pvSpy = (function () {
    var observer = null;
    var navEl = null;
    var linkById = {};

    function setActive(id) {
        Object.keys(linkById).forEach(function (key) {
            linkById[key].classList.toggle("is-active", key === id);
        });
    }

    function onNavClick(e) {
        var a = e.target && e.target.closest ? e.target.closest('a[href*="#"]') : null;
        if (a && a.dataset.spyId) setActive(a.dataset.spyId);
    }

    function init() {
        dispose();
        navEl = document.querySelector(".pv-nav");
        if (!navEl) return;
        var sections = [];
        Array.prototype.forEach.call(navEl.querySelectorAll('a[href*="#"]'), function (a) {
            var parts = a.getAttribute("href").split("#");
            var id = parts[1];
            if (!id) return;
            var el = document.getElementById(id);
            if (!el) return;
            a.dataset.spyId = id;
            linkById[id] = a;
            sections.push(el);
        });
        if (!sections.length) return;
        navEl.addEventListener("click", onNavClick);
        observer = new IntersectionObserver(function (entries) {
            var visible = entries.filter(function (en) { return en.isIntersecting; });
            if (visible.length) {
                visible.sort(function (x, y) { return x.boundingClientRect.top - y.boundingClientRect.top; });
                setActive(visible[0].target.id);
            }
        }, { rootMargin: "-10% 0px -80% 0px" });
        sections.forEach(function (s) { observer.observe(s); });
    }

    function dispose() {
        if (observer) { observer.disconnect(); observer = null; }
        if (navEl) { navEl.removeEventListener("click", onNavClick); navEl = null; }
        linkById = {};
    }

    return { init: init, dispose: dispose };
})();

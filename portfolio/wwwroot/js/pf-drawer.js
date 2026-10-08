/* Portfolio nav drawer helpers: Escape-to-close, scroll lock, focus management. */
window.pfDrawer = (function () {
    var dotNetRef = null;

    function onKeyDown(e) {
        if (e.key === "Escape" && dotNetRef) {
            dotNetRef.invokeMethodAsync("CloseDrawer");
        }
    }

    return {
        init: function (ref) {
            dotNetRef = ref;
            document.addEventListener("keydown", onKeyDown);
        },
        lockScroll: function (lock) {
            document.body.style.overflow = lock ? "hidden" : "";
        },
        focus: function () {
            var el = document.getElementById("pfSidebar");
            if (el) el.focus({ preventScroll: true });
        },
        getRail: function () {
            try { return localStorage.getItem("pf-rail") === "1"; }
            catch (e) { return false; }
        },
        setRail: function (v) {
            try { localStorage.setItem("pf-rail", v ? "1" : "0"); }
            catch (e) { /* private mode */ }
        }
    };
})();

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
        }
    };
})();

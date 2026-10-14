/* v2 keyboard shortcuts: ⌘K / Ctrl+K opens the palette, ⌘; toggles the section nav. */
window.v2keys = (function () {
    var ref = null;
    function onKey(e) {
        var mod = e.metaKey || e.ctrlKey;
        if (mod && e.key.toLowerCase() === 'k') {
            e.preventDefault();
            if (ref) ref.invokeMethodAsync('OpenPaletteFromKey');
        } else if (mod && e.key === ';') {
            e.preventDefault();
            if (ref) ref.invokeMethodAsync('ToggleSectionNavFromKey');
        }
    }
    return {
        init: function (dotNetRef) {
            ref = dotNetRef;
            document.removeEventListener('keydown', onKey);
            document.addEventListener('keydown', onKey);
        }
    };
})();

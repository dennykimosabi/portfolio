// SiteFeedback: point-and-comment annotation tool.
// Activated with ?feedback=1. Clicking any element reports a CSS selector,
// a human-readable label, and the page path back to Blazor.

window.siteFeedback = (() => {
    let dotNet = null;
    let active = false;
    let hoverEl = null;

    const HIGHLIGHT = "2px solid #7c3aed";
    const HOVER_BG = "rgba(124, 58, 237, 0.08)";

    function isFeedbackUI(el) {
        return el && el.closest && el.closest("[data-site-feedback]");
    }

    // Build a short, human-readable selector: tag.class1.class2, with
    // :nth-of-type(n) when siblings share the tag, walking up to 5 levels.
    // Prefers an id when present.
    function selectorFor(el) {
        const parts = [];
        let node = el;
        for (let depth = 0; depth < 5 && node && node !== document.body; depth++) {
            if (node.id) {
                parts.unshift("#" + node.id);
                break;
            }
            let part = node.tagName.toLowerCase();
            const classes = Array.from(node.classList)
                .filter(c => !c.startsWith("sfb-"))
                .slice(0, 3);
            if (classes.length) part += "." + classes.join(".");
            const parent = node.parentElement;
            if (parent) {
                const sameTag = Array.from(parent.children)
                    .filter(c => c.tagName === node.tagName);
                if (sameTag.length > 1) {
                    part += ":nth-of-type(" + (sameTag.indexOf(node) + 1) + ")";
                }
            }
            parts.unshift(part);
            node = parent;
        }
        return parts.join(" > ");
    }

    function labelFor(el) {
        const text = (el.innerText || "").trim().replace(/\s+/g, " ");
        const aria = el.getAttribute && el.getAttribute("aria-label");
        const tag = el.tagName.toLowerCase();
        const cls = Array.from(el.classList).slice(0, 2).join(".");
        const name = aria || (text.length > 60 ? text.slice(0, 60) + "…" : text);
        return tag + (cls ? "." + cls : "") + (name ? ' — "' + name + '"' : "");
    }

    function clearHover() {
        if (hoverEl) {
            hoverEl.style.outline = "";
            hoverEl.style.backgroundColor = "";
            hoverEl = null;
        }
    }

    function onMove(e) {
        if (isFeedbackUI(e.target)) { clearHover(); return; }
        clearHover();
        hoverEl = e.target;
        if (hoverEl && hoverEl !== document.body) {
            hoverEl.style.outline = HIGHLIGHT;
            hoverEl.style.backgroundColor = HOVER_BG;
        }
    }

    function onClick(e) {
        if (!active || isFeedbackUI(e.target)) return;
        e.preventDefault();
        e.stopPropagation();
        const el = e.target;
        clearHover();
        dotNet.invokeMethodAsync("OnElementPicked", {
            selector: selectorFor(el),
            label: labelFor(el),
            page: window.location.pathname
        });
    }

    function onKey(e) {
        if (e.key === "Escape") dotNet.invokeMethodAsync("CancelPick");
    }

    return {
        start(ref) {
            dotNet = ref;
            active = true;
            document.body.style.cursor = "crosshair";
            document.addEventListener("mousemove", onMove, true);
            document.addEventListener("click", onClick, true);
            document.addEventListener("keydown", onKey, true);
        },
        stop() {
            active = false;
            clearHover();
            document.body.style.cursor = "";
            document.removeEventListener("mousemove", onMove, true);
            document.removeEventListener("click", onClick, true);
            document.removeEventListener("keydown", onKey, true);
        },
        copyText(text) {
            return navigator.clipboard.writeText(text);
        }
    };
})();

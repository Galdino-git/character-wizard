// Minimal JS helper for the EntryDisplay component.
//   - getRefDataFromTarget: invoked from .NET to read data-cw-* attributes of
//     whatever the user actually clicked (since the @onclick handler fires on
//     the container, not the inner span).
//   - positionTooltip: keeps the CSS pseudo-tooltip near the cursor without
//     needing JS state in .NET.
window.cwRef = window.cwRef || {};

window.cwRef.getRefDataFromTarget = function (containerSelector, event) {
    // Fallback path: query selector + walk up from event source
    const container = document.querySelector(containerSelector);
    if (!container) return null;
    // .NET passes the event with composedPath via the MouseEventArgs serialization;
    // we rely on the standard browser event in window.event as a fallback.
    let el = (event && event.target) || (window.event && window.event.target);
    while (el && el !== container) {
        if (el.classList && el.classList.contains('cw-ref')) {
            return {
                category: el.dataset.cwCat || '',
                name:     el.dataset.cwName || '',
                source:   el.dataset.cwSource || ''
            };
        }
        el = el.parentElement;
    }
    return null;
};

// Position the tooltip via CSS custom properties using current cursor coords.
// Attached once at startup; works for any .cw-ref currently or later in the DOM.
document.addEventListener('mousemove', function (e) {
    const target = e.target;
    if (target && target.classList && target.classList.contains('cw-ref')) {
        target.style.setProperty('--cw-tip-x', e.clientX + 'px');
        target.style.setProperty('--cw-tip-y', e.clientY + 'px');
    }
}, { passive: true });

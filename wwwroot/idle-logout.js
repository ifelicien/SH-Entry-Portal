// Auto-logout after 5 minutes of no user activity (mouse, keyboard, scroll, touch)
(function () {
    const idleLimitMs = 5 * 60 * 1000; // 5 minutes
    let lastActivity = Date.now();

    ['mousemove', 'mousedown', 'keydown', 'scroll', 'touchstart'].forEach(function (evt) {
        window.addEventListener(evt, function () { lastActivity = Date.now(); }, { passive: true });
    });

    setInterval(function () {
        if (Date.now() - lastActivity >= idleLimitMs) {
            fetch('/auth/logout', { method: 'POST' }).finally(function () {
                window.location.href = '/login';
            });
        }
    }, 15000);

    // Clicking any same-origin link (including NavLinks that cause a full page transition
    // between differently-rendered pages) is treated as intentional in-app navigation, not a close.
    // sessionStorage survives a full page navigation within the same tab, unlike a plain JS variable.
    document.addEventListener('click', function (e) {
        var link = e.target.closest('a[href]');
        if (link && link.origin === window.location.origin) {
            sessionStorage.setItem('sh_internal_nav', '1');
        }
    }, true);

    document.addEventListener('submit', function (e) {
        var form = e.target.closest('form');
        if (form && (!form.action || new URL(form.action, window.location.href).origin === window.location.origin)) {
            sessionStorage.setItem('sh_internal_nav', '1');
        }
    }, true);

    // Fires on tab/window close, refresh, or navigating away entirely (unlike fetch, which
    // browsers can cancel mid-unload). Skipped if the page is unloading due to our own link/form.
    window.addEventListener('pagehide', function () {
        if (sessionStorage.getItem('sh_internal_nav') === '1') {
            sessionStorage.removeItem('sh_internal_nav');
            return;
        }
        navigator.sendBeacon('/auth/logout');
    });
})();

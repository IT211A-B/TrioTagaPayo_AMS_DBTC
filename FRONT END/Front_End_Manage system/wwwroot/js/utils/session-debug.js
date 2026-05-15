// session-debug.js - Debug session issues
(function () {
    // Check session on page load
    fetch('/Admin/GetCurrentUserInfo')
        .then(function (res) { return res.json(); })
        .then(function (data) {
            console.log('Session Debug - Page Load:', data);
            if (!data.isLoggedIn) {
                console.error('Session Debug: NOT LOGGED IN on page load!');
            }
        })
        .catch(function (err) {
            console.error('Session Debug - Fetch failed:', err);
        });

    // Monitor all navigation clicks
    document.addEventListener('click', function (e) {
        var link = e.target.closest('a');
        if (link && link.href && !link.href.includes('Logout')) {
            console.log('Session Debug - Navigating to:', link.href);
        }
    });
})();
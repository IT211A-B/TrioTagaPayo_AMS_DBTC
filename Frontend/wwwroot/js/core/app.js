// Core app initialization
document.addEventListener('DOMContentLoaded', function () {
    console.log('AMS initialized');
    initSidebar();
    initModals();
    initSessionDebug();
});

function initSidebar() {
    const sidebar = document.getElementById('sidebar');
    const overlay = document.getElementById('sidebarOverlay');

    window.toggleSidebar = function () {
        if (sidebar) sidebar.classList.toggle('open');
        if (overlay) overlay.classList.toggle('visible');
    };

    window.closeSidebar = function () {
        if (sidebar) sidebar.classList.remove('open');
        if (overlay) overlay.classList.remove('visible');
    };

    // Close sidebar when clicking a link on mobile
    document.querySelectorAll('.sidebar .nav-link').forEach(link => {
        link.addEventListener('click', function (e) {
            if (window.innerWidth <= 900) {
                setTimeout(function () {
                    window.closeSidebar();
                }, 100);
            }
        });
    });
}

function initModals() {
    document.querySelectorAll('.modal-overlay').forEach(overlay => {
        overlay.addEventListener('click', function (e) {
            if (e.target === this) this.classList.remove('active');
        });
    });

    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape') {
            document.querySelectorAll('.modal-overlay.active').forEach(m => m.classList.remove('active'));
        }
    });
}

window.openModal = function (id) {
    const modal = document.getElementById(id);
    if (modal) modal.classList.add('active');
};

window.closeModal = function (id) {
    const modal = document.getElementById(id);
    if (modal) modal.classList.remove('active');
};

// Session debug - helps identify logout issues
function initSessionDebug() {
    // Check session every 30 seconds
    setInterval(function () {
        fetch('/Admin/GetCurrentUserInfo', {
            headers: {
                'RequestVerificationToken': getAntiForgeryToken()
            }
        })
            .then(res => res.json())
            .then(data => {
                if (!data.isLoggedIn) {
                    console.warn('⚠️ Session lost at:', new Date().toLocaleTimeString());
                    // Don't auto-redirect, just log
                } else {
                    console.log('✅ Session active - Role:', data.role);
                }
            })
            .catch(err => console.log('Session check failed:', err));
    }, 30000);
}

// Helper to get anti-forgery token
function getAntiForgeryToken() {
    const token = document.querySelector('input[name="__RequestVerificationToken"]');
    return token ? token.value : '';
}
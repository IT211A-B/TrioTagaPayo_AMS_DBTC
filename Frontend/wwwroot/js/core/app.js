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

    document.querySelectorAll('.sidebar .nav-link').forEach(link => {
        link.addEventListener('click', function () {
            if (window.innerWidth <= 900) {
                setTimeout(window.closeSidebar, 100);
            }
        });
    });
}

// SINGLE SOURCE OF TRUTH FOR MODAL FUNCTIONS
window.openModal = function (id) {
    const modal = document.getElementById(id);
    if (modal) {
        modal.classList.add('active');
        console.log('Modal opened:', id);
    } else {
        console.error('Modal not found:', id);
    }
};

window.closeModal = function (id) {
    const modal = document.getElementById(id);
    if (modal) {
        modal.classList.remove('active');
        console.log('Modal closed:', id);
    }
};

function initModals() {
    document.querySelectorAll('.modal-overlay').forEach(overlay => {
        overlay.addEventListener('click', function (e) {
            if (e.target === this) {
                const id = this.id;
                if (id && window.closeModal) window.closeModal(id);
            }
        });
    });

    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape') {
            document.querySelectorAll('.modal-overlay.active').forEach(modal => {
                if (window.closeModal) window.closeModal(modal.id);
            });
        }
    });
}

// Session debug
function initSessionDebug() {
    setInterval(function () {
        fetch('/Admin/GetCurrentUserInfo', {
            headers: { 'RequestVerificationToken': getAntiForgeryToken() }
        })
            .then(res => res.json())
            .then(data => {
                if (!data.isLoggedIn) {
                    console.warn('⚠️ Session lost at:', new Date().toLocaleTimeString());
                } else {
                    console.log('✅ Session active - Role:', data.role);
                }
            })
            .catch(err => console.log('Session check failed:', err));
    }, 30000);
}

function getAntiForgeryToken() {
    const token = document.querySelector('input[name="__RequestVerificationToken"]');
    return token ? token.value : '';
}
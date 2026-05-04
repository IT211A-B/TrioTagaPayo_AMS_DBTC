// toast.js — Global toast alert system
window.showToast = function (message, type = 'info', duration = 3500) {
    const container = document.getElementById('toastContainer');
    if (!container) return;

    const icons = { success: '✓', error: '✕', warning: '⚠', info: 'ℹ' };
    const labels = { success: 'Success', error: 'Error', warning: 'Warning', info: 'Info' };

    const toast = document.createElement('div');
    toast.className = `toast toast-${type}`;
    toast.innerHTML = `
        <div class="toast-icon">${icons[type] || 'ℹ'}</div>
        <div class="toast-body">
            <p class="toast-label">${labels[type] || 'Info'}</p>
            <p class="toast-message">${escapeHtml(message)}</p>
        </div>
        <button class="toast-close" onclick="window.dismissToast(this)">×</button>
        <div class="toast-progress"></div>`;

    container.appendChild(toast);
    setTimeout(() => toast.classList.add('toast-show'), 10);

    toast._timer = setTimeout(() => window.dismissToast(toast.querySelector('.toast-close')), duration);
};

window.dismissToast = function (btn) {
    const toast = btn.closest('.toast');
    if (!toast) return;
    clearTimeout(toast._timer);
    toast.classList.remove('toast-show');
    toast.classList.add('toast-hide');
    toast.addEventListener('transitionend', () => toast.remove(), { once: true });
};

function escapeHtml(str) {
    if (!str) return '';
    return str.replace(/[&<>]/g, function (m) {
        if (m === '&') return '&amp;';
        if (m === '<') return '&lt;';
        if (m === '>') return '&gt;';
        return m;
    });
}

window.Toast = {
    success: (m) => window.showToast(m, 'success'),
    error: (m) => window.showToast(m, 'error'),
    warning: (m) => window.showToast(m, 'warning'),
    info: (m) => window.showToast(m, 'info'),
}; 
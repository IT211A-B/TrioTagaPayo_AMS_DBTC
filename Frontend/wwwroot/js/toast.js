// toast.js — Global toast alert system
function showToast(message, type = 'info', duration = 3500) {
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
            <p class="toast-message">${message}</p>
        </div>
        <button class="toast-close" onclick="dismissToast(this)">×</button>
        <div class="toast-progress"></div>`;

    container.appendChild(toast);
    requestAnimationFrame(() => requestAnimationFrame(() => toast.classList.add('toast-show')));

    toast._timer = setTimeout(() => dismissToast(toast.querySelector('.toast-close')), duration);
}

function dismissToast(btn) {
    const toast = btn.closest('.toast');
    if (!toast) return;
    clearTimeout(toast._timer);
    toast.classList.remove('toast-show');
    toast.classList.add('toast-hide');
    toast.addEventListener('transitionend', () => toast.remove(), { once: true });
}

const Toast = {
    success: m => showToast(m, 'success'),
    error: m => showToast(m, 'error'),
    warning: m => showToast(m, 'warning'),
    info: m => showToast(m, 'info'),
};
// ── CLOCK ─────────────────────────────────────────────────
function updateClock() {
    const el = document.getElementById('clock');
    if (!el) return;
    const now = new Date();
    el.textContent = now.toLocaleTimeString('en-US', {
        hour: '2-digit', minute: '2-digit', second: '2-digit'
    });
}
setInterval(updateClock, 1000);
updateClock();

// ── P/A/L TOGGLE ──────────────────────────────────────────
function setPal(btn, type) {
    const group = btn.closest('.pal-group');
    group.querySelectorAll('.pal-btn').forEach(b =>
        b.classList.remove('p-active', 'a-active', 'l-active'));
    if (type === 'p') btn.classList.add('p-active');
    if (type === 'a') btn.classList.add('a-active');
    if (type === 'l') btn.classList.add('l-active');
}

// ── TAB TOGGLE ────────────────────────────────────────────
document.querySelectorAll('.tab-btn').forEach(btn => {
    btn.addEventListener('click', () => {
        btn.closest('.tab-group').querySelectorAll('.tab-btn')
            .forEach(b => b.classList.remove('active'));
        btn.classList.add('active');
    });
});

// ── DARK/LIGHT THEME TOGGLE ───────────────────────────────
// Toggle UI: uiverse.io/alexruix/splendid-liger-23
// ─────────────────────────────────────────────────────────
const themeToggle = document.getElementById('themeToggle');

if (localStorage.getItem('theme') === 'light') {
    document.body.classList.add('light-mode');
    if (themeToggle) themeToggle.checked = true;
}

if (themeToggle) {
    themeToggle.addEventListener('change', () => {
        if (themeToggle.checked) {
            document.body.classList.add('light-mode');
            localStorage.setItem('theme', 'light');
        } else {
            document.body.classList.remove('light-mode');
            localStorage.setItem('theme', 'dark');
        }
    });
}

// ── PAGE LOADING SCREEN ───────────────────────────────────
// Dots: codepen.io/nzbin/pen/GGrXbp (MIT)
// ─────────────────────────────────────────────────────────
window.addEventListener('load', () => {
    const loader = document.getElementById('pageLoader');
    if (!loader) return;
    setTimeout(() => {
        loader.classList.add('hidden');
        setTimeout(() => loader.remove(), 400);
    }, 800);
});

// ── HAMBURGER / MOBILE SIDEBAR ────────────────────────────
const hamburgerBtn = document.getElementById('hamburgerBtn');
const sidebarOverlay = document.getElementById('sidebarOverlay');
const sidebar = document.querySelector('.sidebar');

if (hamburgerBtn && sidebar) {
    hamburgerBtn.addEventListener('click', () => {
        hamburgerBtn.classList.toggle('open');
        sidebar.classList.toggle('open');
        sidebarOverlay.classList.toggle('show');
    });

    sidebarOverlay.addEventListener('click', () => {
        hamburgerBtn.classList.remove('open');
        sidebar.classList.remove('open');
        sidebarOverlay.classList.remove('show');
    });
}

// ── MODAL HELPERS ─────────────────────────────────────────
function openModal(id) {
    document.getElementById(id).classList.add('show');
}

function closeModal(id) {
    document.getElementById(id).classList.remove('show');
}

// Close modal on backdrop click
document.querySelectorAll('.modal-backdrop').forEach(backdrop => {
    backdrop.addEventListener('click', (e) => {
        if (e.target === backdrop) backdrop.classList.remove('show');
    });
});

// ── LOGOUT CONFIRMATION ───────────────────────────────────
const logoutBtn = document.getElementById('logoutBtn');
if (logoutBtn) {
    logoutBtn.addEventListener('click', () => openModal('logoutModal'));
}

// ── TOAST NOTIFICATIONS ───────────────────────────────────
// Inspired by uiverse.io toast styles
// Source: https://uiverse.io/toasts
// Usage: showToast('message', 'success') or 'error', 'warning', 'info'
// ─────────────────────────────────────────────────────────
function showToast(message, type = 'info', duration = 3000) {
    let container = document.querySelector('.toast-container');
    if (!container) {
        container = document.createElement('div');
        container.className = 'toast-container';
        document.body.appendChild(container);
    }

    const icons = {
        success: 'fa-check',
        error: 'fa-times',
        warning: 'fa-exclamation',
        info: 'fa-info'
    };

    const toast = document.createElement('div');
    toast.className = `toast ${type}`;
    toast.innerHTML = `
        <div class="toast-icon"><i class="fas ${icons[type] || 'fa-info'}"></i></div>
        <span class="toast-msg">${message}</span>
        <button class="toast-close" onclick="removeToast(this.parentElement)">×</button>
    `;

    container.appendChild(toast);

    setTimeout(() => removeToast(toast), duration);
}

function removeToast(toast) {
    if (!toast) return;
    toast.classList.add('hiding');
    setTimeout(() => toast.remove(), 300);
}

// ── EDIT STUDENT MODAL ────────────────────────────────────
function openEditStudent(no, name, section) {
    document.getElementById('editStudentNo').value = no;
    document.getElementById('editStudentName').value = name;
    document.getElementById('editStudentSec').value = section;
    openModal('editStudentModal');
}

function saveEditStudent() {
    closeModal('editStudentModal');
    showToast('Student updated successfully!', 'success');
}

// ── DELETE STUDENT MODAL ──────────────────────────────────
function openDeleteStudent(name) {
    document.getElementById('deleteStudentName').textContent = name;
    openModal('deleteStudentModal');
}

function confirmDeleteStudent() {
    closeModal('deleteStudentModal');
    showToast('Student deleted.', 'error');
}

// ── EDIT TEACHER MODAL ────────────────────────────────────
function openEditTeacher(id, name, email) {
    document.getElementById('editTeacherId').value = id;
    document.getElementById('editTeacherName').value = name;
    document.getElementById('editTeacherEmail').value = email;
    openModal('editTeacherModal');
}

function saveEditTeacher() {
    closeModal('editTeacherModal');
    showToast('Teacher updated successfully!', 'success');
}

// ── DISABLE/ENABLE TEACHER ────────────────────────────────
function confirmDisableTeacher(name) {
    document.getElementById('disableTeacherName').textContent = name;
    openModal('disableTeacherModal');
}

function confirmAction() {
    closeModal('disableTeacherModal');
    showToast('Teacher status updated.', 'warning');
}
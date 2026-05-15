// site.js — sidebar toggle for mobile
function toggleSidebar() {
    document.getElementById('sidebar').classList.toggle('open');
    document.getElementById('sidebarOverlay').classList.toggle('visible');
}
function closeSidebar() {
    document.getElementById('sidebar').classList.remove('open');
    document.getElementById('sidebarOverlay').classList.remove('visible');
}
document.addEventListener('DOMContentLoaded', () => {
    document.querySelectorAll('.sidebar .nav-link').forEach(link => {
        link.addEventListener('click', () => { if (window.innerWidth <= 768) closeSidebar(); });
    });
});
// Helper functions
function getInitials(firstName, lastName) {
    return (firstName?.charAt(0) || '') + (lastName?.charAt(0) || '');
}

function formatDate(dateString) {
    if (!dateString) return '';
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', { year: 'numeric', month: 'short', day: 'numeric' });
}

function getAvatarColor(id) {
    const colors = [
        'linear-gradient(135deg,#c2185b,#e91e8c)',
        'linear-gradient(135deg,#7c3aed,#a855f7)',
        'linear-gradient(135deg,#0891b2,#06b6d4)',
        'linear-gradient(135deg,#dc2626,#ef4444)',
        'linear-gradient(135deg,#d97706,#f59e0b)'
    ];
    return colors[(id - 1) % colors.length];
}

function debounce(func, wait) {
    let timeout;
    return function executedFunction(...args) {
        const later = () => {
            clearTimeout(timeout);
            func(...args);
        };
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
    };
}   
// helpers.js — Utility functions

// Format date to YYYY-MM-DD
function formatDate(date) {
    if (!date) return '';
    const d = new Date(date);
    const year = d.getFullYear();
    const month = String(d.getMonth() + 1).padStart(2, '0');
    const day = String(d.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
}

// Format datetime to readable string
function formatDateTime(date) {
    if (!date) return '';
    const d = new Date(date);
    return d.toLocaleString();
}

// Debounce function for search inputs
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

// Get query parameter from URL
function getQueryParam(param) {
    const urlParams = new URLSearchParams(window.location.search);
    return urlParams.get(param);
}

// Reload table via AJAX
function reloadTable(tableId, url, data) {
    fetch(url, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value || ''
        },
        body: JSON.stringify(data)
    })
        .then(response => response.text())
        .then(html => {
            const tableBody = document.querySelector(`#${tableId} tbody`);
            if (tableBody) tableBody.innerHTML = html;
        })
        .catch(error => console.error('Error reloading table:', error));
}
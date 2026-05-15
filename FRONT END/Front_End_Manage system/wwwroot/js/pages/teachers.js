// teachers.js - Teachers page JavaScript

let currentPage = 1;
let isLoading = false;
let searchTimer = null;
let isEditing = false;
let pendingDeleteId = null;
let antiForgeryToken = '';

document.addEventListener('DOMContentLoaded', function () {
    var tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
    if (tokenInput) antiForgeryToken = tokenInput.value;

    var searchInput = document.getElementById('searchInput');
    if (searchInput) searchInput.addEventListener('input', handleSearch);

    var statusFilter = document.getElementById('statusFilter');
    if (statusFilter) statusFilter.addEventListener('change', handleSearch);

    document.querySelectorAll('.modal-overlay').forEach(function (o) {
        o.addEventListener('click', function (e) {
            if (e.target === o) o.classList.remove('active');
        });
    });

    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape') {
            document.querySelectorAll('.modal-overlay.active').forEach(function (m) {
                m.classList.remove('active');
            });
        }
    });
});

// FIXED: Updated handleSearch to work with TeachersPartial endpoint (no page parameter needed)
function handleSearch() {
    clearTimeout(searchTimer);
    searchTimer = setTimeout(async function () {
        var searchValue = document.getElementById('searchInput').value;
        var statusValue = document.getElementById('statusFilter').value;
        try {
            // FIX: Use TeachersPartial endpoint without page parameter
            var res = await fetch('/Admin/TeachersPartial?search=' + encodeURIComponent(searchValue) + '&status=' + encodeURIComponent(statusValue));
            var html = await res.text();
            document.getElementById('teachersBody').innerHTML = html || emptyRow();
            currentPage = 1;
        } catch (error) {
            console.error('Search failed:', error);
            if (typeof Toast !== 'undefined') Toast.error('Search failed.');
        }
    }, 350);
}

function emptyRow() {
    return '<tr><td colspan="6"><div class="empty-state"><span class="empty-icon">◈</span><p class="empty-title">No teachers found</p><p class="empty-message">Try a different search.</p></div></td></tr>';
}

function openAddModal() {
    isEditing = false;
    document.getElementById('modalTitle').textContent = 'Add Teacher';
    clearForm();
    openModal('teacherModal');
}

function editTeacher(id, tno, fn, ln, email) {
    isEditing = true;
    document.getElementById('modalTitle').textContent = 'Edit Teacher';
    document.getElementById('dbId').value = id;
    document.getElementById('teacherNo').value = tno;
    document.getElementById('firstName').value = fn;
    document.getElementById('lastName').value = ln;
    document.getElementById('email').value = email;
    openModal('teacherModal');
}

function viewTeacher(id, name, tno, email, courses, username, isActive) {
    var status = isActive === 'true' ? 'Active' : 'Inactive';
    document.getElementById('viewBody').innerHTML = `
        <div class="profile-view">
            <div class="profile-avatar" style="background:linear-gradient(135deg,#d81b60,#9c1040);color:#fff;">${name.charAt(0)}</div>
            <h3 class="profile-name">${escapeHtml(name)}</h3>
            <p class="profile-id">${escapeHtml(tno)}</p>
            <div class="profile-details">
                <div class="detail-row"><span class="detail-label">Email</span><span>${escapeHtml(email)}</span></div>
                <div class="detail-row"><span class="detail-label">Courses</span><span>${courses} assigned</span></div>
                <div class="detail-row"><span class="detail-label">Username</span><span>${escapeHtml(username) || '—'}</span></div>
                <div class="detail-row"><span class="detail-label">Status</span>
                    <span class="status-badge ${status === 'Active' ? 'badge-active' : 'badge-inactive'}">● ${status}</span>
                </div>
            </div>
        </div>`;
    openModal('viewModal');
}

function confirmDelete(id, name) {
    pendingDeleteId = id;
    document.getElementById('deleteName').textContent = name;
    openModal('deleteModal');
}

async function submitTeacher() {
    var fn = document.getElementById('firstName').value.trim();
    var ln = document.getElementById('lastName').value.trim();
    var em = document.getElementById('email').value.trim();

    if (!fn || !ln || !em) {
        if (typeof Toast !== 'undefined') Toast.warning('Please fill in all required fields.');
        return;
    }

    var btn = document.getElementById('saveBtn');
    btn.disabled = true;
    btn.textContent = 'Saving...';

    var url = isEditing ? '/Admin/UpdateTeacher' : '/Admin/AddTeacher';
    var payload = new URLSearchParams();
    payload.append('__RequestVerificationToken', antiForgeryToken);
    payload.append('id', document.getElementById('dbId').value);
    payload.append('firstName', fn);
    payload.append('lastName', ln);
    payload.append('email', em);

    try {
        var response = await fetch(url, {
            method: 'POST',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded', 'X-Requested-With': 'XMLHttpRequest' },
            body: payload
        });
        var data = await response.json();
        if (data.success) {
            if (typeof Toast !== 'undefined') Toast.success(data.message);
            closeModal('teacherModal');
            location.reload();
        } else {
            if (typeof Toast !== 'undefined') Toast.error(data.message);
        }
    } catch (error) {
        console.error('Submit teacher error:', error);
        if (typeof Toast !== 'undefined') Toast.error('An unexpected error occurred.');
    } finally {
        btn.disabled = false;
        btn.textContent = 'Save Teacher';
    }
}

async function submitDelete() {
    var btn = document.getElementById('confirmDeleteBtn');
    btn.disabled = true;
    btn.textContent = 'Deleting...';

    var payload = new URLSearchParams();
    payload.append('__RequestVerificationToken', antiForgeryToken);
    payload.append('id', pendingDeleteId);

    try {
        var response = await fetch('/Admin/DeleteTeacher', {
            method: 'POST',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded', 'X-Requested-With': 'XMLHttpRequest' },
            body: payload
        });
        var data = await response.json();
        if (data.success) {
            if (typeof Toast !== 'undefined') Toast.success(data.message);
            closeModal('deleteModal');
            var row = document.getElementById('row-' + pendingDeleteId);
            if (row) row.remove();
        } else {
            if (typeof Toast !== 'undefined') Toast.error(data.message);
        }
    } catch (error) {
        console.error('Delete teacher error:', error);
        if (typeof Toast !== 'undefined') Toast.error('An unexpected error occurred.');
    } finally {
        btn.disabled = false;
        btn.textContent = 'Yes, Delete';
    }
}

function clearForm() {
    var ids = ['dbId', 'teacherNo', 'firstName', 'lastName', 'email'];
    for (var i = 0; i < ids.length; i++) {
        var el = document.getElementById(ids[i]);
        if (el) el.value = '';
    }
}

function openModal(id) {
    var modal = document.getElementById(id);
    if (modal) modal.classList.add('active');
}

function closeModal(id) {
    var modal = document.getElementById(id);
    if (modal) modal.classList.remove('active');
}

function escapeHtml(str) {
    if (!str) return '';
    return str.replace(/[&<>]/g, function (m) {
        if (m === '&') return '&amp;';
        if (m === '<') return '&lt;';
        if (m === '>') return '&gt;';
        return m;
    });
}
// students.js - Students page JavaScript

let isEditing = false;
let pendingDeleteId = null;
let antiForgeryToken = '';

document.addEventListener('DOMContentLoaded', function () {
    var tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
    if (tokenInput) antiForgeryToken = tokenInput.value;

    var searchInput = document.getElementById('searchInput');
    if (searchInput) searchInput.addEventListener('input', filterStudents);

    var sectionFilter = document.getElementById('sectionFilter');
    if (sectionFilter) sectionFilter.addEventListener('change', filterStudents);

    var statusFilter = document.getElementById('statusFilter');
    if (statusFilter) statusFilter.addEventListener('change', filterStudents);

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

function filterStudents() {
    var searchTerm = document.getElementById('searchInput').value.toLowerCase();
    var sectionFilter = document.getElementById('sectionFilter').value;
    var rows = document.querySelectorAll('#studentsBody tr.table-row');
    var visibleCount = 0;

    rows.forEach(function (row) {
        var text = row.textContent.toLowerCase();
        var section = row.querySelector('td:nth-child(3)')?.textContent || '';
        var show = text.includes(searchTerm);
        if (show && sectionFilter && section !== sectionFilter) show = false;
        row.style.display = show ? '' : 'none';
        if (show) visibleCount++;
    });

    var endSpan = document.getElementById('showingEnd');
    if (endSpan) endSpan.innerText = visibleCount;
}

function openAddModal() {
    isEditing = false;
    document.getElementById('modalTitle').textContent = 'Add Student';
    clearForm();
    openModal('studentModal');
}

function editStudent(btn) {
    isEditing = true;
    var row = btn.closest('tr');
    document.getElementById('modalTitle').textContent = 'Edit Student';
    document.getElementById('dbId').value = row.getAttribute('data-id');
    document.getElementById('studentNo').value = row.getAttribute('data-studentno');
    document.getElementById('firstName').value = row.getAttribute('data-firstname');
    document.getElementById('middleName').value = row.getAttribute('data-middlename') || '';
    document.getElementById('lastName').value = row.getAttribute('data-lastname');
    document.getElementById('email').value = row.getAttribute('data-email');
    document.getElementById('section').value = row.getAttribute('data-section');
    document.getElementById('mobileNo').value = row.getAttribute('data-mobile') || '';
    openModal('studentModal');
}

function viewStudent(btn) {
    var row = btn.closest('tr');
    var name = row.getAttribute('data-fullname');
    var sno = row.getAttribute('data-studentno');
    var email = row.getAttribute('data-email');
    var section = row.getAttribute('data-section');
    var mobile = row.getAttribute('data-mobile');

    var viewBody = document.getElementById('viewBody');
    if (viewBody) {
        viewBody.innerHTML = '<div class="profile-view">' +
            '<div class="profile-avatar">' + (name ? name.charAt(0) : '?') + '</div>' +
            '<h3 class="profile-name">' + escapeHtml(name) + '</h3>' +
            '<p class="profile-id">' + escapeHtml(sno) + '</p>' +
            '<div class="profile-details">' +
            '<div class="detail-row"><span class="detail-label">Email</span><span>' + escapeHtml(email) + '</span></div>' +
            '<div class="detail-row"><span class="detail-label">Section</span><span>' + escapeHtml(section) + '</span></div>' +
            '<div class="detail-row"><span class="detail-label">Mobile</span><span>' + (mobile ? escapeHtml(mobile) : '—') + '</span></div>' +
            '</div>' +
            '</div>';
    }
    openModal('viewModal');
}

function confirmDelete(btn) {
    var row = btn.closest('tr');
    pendingDeleteId = row.getAttribute('data-id');
    var name = row.getAttribute('data-fullname');
    var deleteName = document.getElementById('deleteName');
    if (deleteName) deleteName.textContent = name;
    openModal('deleteModal');
}

async function submitStudent() {
    var firstName = document.getElementById('firstName').value.trim();
    var lastName = document.getElementById('lastName').value.trim();
    var email = document.getElementById('email').value.trim();
    var section = document.getElementById('section').value.trim();

    if (!firstName || !lastName || !email || !section) {
        if (typeof Toast !== 'undefined') Toast.warning('Please fill in all required fields.');
        return;
    }

    var saveBtn = document.getElementById('saveBtn');
    if (saveBtn) {
        saveBtn.disabled = true;
        saveBtn.textContent = 'Saving...';
    }

    var url = isEditing ? '/Admin/UpdateStudent' : '/Admin/AddStudent';
    var payload = new URLSearchParams();
    payload.append('__RequestVerificationToken', antiForgeryToken);
    payload.append('id', document.getElementById('dbId').value);
    payload.append('studentNo', document.getElementById('studentNo').value);
    payload.append('firstName', firstName);
    payload.append('middleName', document.getElementById('middleName').value.trim());
    payload.append('lastName', lastName);
    payload.append('email', email);
    payload.append('section', section);
    payload.append('mobileNo', document.getElementById('mobileNo').value.trim());

    try {
        var response = await fetch(url, {
            method: 'POST',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded', 'X-Requested-With': 'XMLHttpRequest' },
            body: payload
        });
        var data = await response.json();
        if (data.success) {
            if (typeof Toast !== 'undefined') Toast.success(data.message);
            closeModal('studentModal');
            location.reload();
        } else {
            if (typeof Toast !== 'undefined') Toast.error(data.message);
        }
    } catch (error) {
        if (typeof Toast !== 'undefined') Toast.error('An unexpected error occurred.');
    } finally {
        if (saveBtn) {
            saveBtn.disabled = false;
            saveBtn.textContent = 'Save Student';
        }
    }
}

async function submitDelete() {
    var confirmBtn = document.getElementById('confirmDeleteBtn');
    if (confirmBtn) {
        confirmBtn.disabled = true;
        confirmBtn.textContent = 'Deleting...';
    }

    var payload = new URLSearchParams();
    payload.append('__RequestVerificationToken', antiForgeryToken);
    payload.append('id', pendingDeleteId);

    try {
        var response = await fetch('/Admin/DeleteStudent', {
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
        if (typeof Toast !== 'undefined') Toast.error('An unexpected error occurred.');
    } finally {
        if (confirmBtn) {
            confirmBtn.disabled = false;
            confirmBtn.textContent = 'Yes, Delete';
        }
    }
}

function clearForm() {
    ['dbId', 'studentNo', 'firstName', 'middleName', 'lastName', 'email', 'section', 'mobileNo'].forEach(function (id) {
        var el = document.getElementById(id);
        if (el) el.value = '';
    });
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
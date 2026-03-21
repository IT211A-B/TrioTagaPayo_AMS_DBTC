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
        if (sidebarOverlay) sidebarOverlay.classList.toggle('show');
    });

    if (sidebarOverlay) {
        sidebarOverlay.addEventListener('click', () => {
            hamburgerBtn.classList.remove('open');
            sidebar.classList.remove('open');
            sidebarOverlay.classList.remove('show');
        });
    }
}

// ── MODAL HELPERS ─────────────────────────────────────────
function openModal(id) {
    const el = document.getElementById(id);
    if (el) el.classList.add('show');
}

function closeModal(id) {
    const el = document.getElementById(id);
    if (el) el.classList.remove('show');
}

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

// ── RESET ATTENDANCE ──────────────────────────────────────
function resetAttendance() {
    document.querySelectorAll('#attBody tr').forEach(row => {
        const btns = row.querySelectorAll('.pal-btn');
        btns.forEach(b => b.classList.remove('p-active', 'a-active', 'l-active'));
        if (btns[0]) btns[0].classList.add('p-active');
        const ri = row.querySelector('.remarks-input');
        if (ri) ri.value = '';
    });
    showToast('Attendance reset.', 'info');
}

// ── FILTER STUDENTS BY COURSE SECTION ─────────────────────
function filterStudentsByCourse() {
    const sel = document.getElementById('courseSelect');
    if (!sel) return;
    const section = sel.options[sel.selectedIndex]?.dataset.section ?? '';
    const rows = document.querySelectorAll('#attBody tr');
    let n = 1;
    rows.forEach(row => {
        const show = !section || (row.dataset.section ?? '') === section;
        row.style.display = show ? '' : 'none';
        if (show) {
            const nc = row.querySelector('.num-col');
            if (nc) nc.textContent = n++;
        }
    });
}

// ── SAVE ATTENDANCE (AJAX POST to backend) ─────────────────
async function saveAttendance() {
    const courseSelect = document.getElementById('courseSelect');
    const dateInput = document.getElementById('attDate');

    if (!courseSelect || !dateInput) {
        showToast('Missing course or date field.', 'error');
        return;
    }

    const courseId = parseInt(courseSelect.value);
    const date = dateInput.value;

    if (!date) {
        showToast('Please select a date.', 'error');
        return;
    }

    const rows = document.querySelectorAll('#attBody tr');
    const records = [];

    rows.forEach(row => {
        if (row.style.display === 'none') return;
        const studentId = parseInt(row.dataset.studentId);
        if (!studentId) return;

        const active = row.querySelector('.pal-btn.p-active, .pal-btn.a-active, .pal-btn.l-active');
        const letter = active ? active.textContent.trim() : 'P';
        const status = letter === 'P' ? 'Present' : letter === 'A' ? 'Absent' : 'Late';
        const remarks = row.querySelector('.remarks-input')?.value ?? '';

        records.push({ studentId, courseId, date, status, remarks });
    });

    if (records.length === 0) {
        showToast('No students to save.', 'error');
        return;
    }

    const isTeacher = window.location.pathname.toLowerCase().includes('teacher');
    const controller = isTeacher ? 'TeacherDashboard' : 'AdminDashboard';

    try {
        const res = await fetch(`/${controller}/SaveAttendance`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(records)
        });

        if (res.ok) {
            showToast(`Attendance saved for ${records.length} students!`, 'success');
        } else {
            showToast('Failed to save attendance. Try again.', 'error');
        }
    } catch {
        showToast('Cannot reach server. Is the backend running?', 'error');
    }
}

// ── FILTER ATTENDANCE RECORDS (Teacher Records page) ───────
async function filterRecords() {
    const courseId = document.getElementById('recordCourseSelect')?.value;
    const from = document.getElementById('fromDate')?.value;
    const to = document.getElementById('toDate')?.value;

    if (!courseId || !from || !to) {
        showToast('Please fill in all filter fields.', 'error');
        return;
    }

    const tbody = document.getElementById('recordsBody');
    tbody.innerHTML = `<tr><td colspan="5" style="text-align:center;color:var(--muted);padding:24px;">Loading...</td></tr>`;

    try {
        const res = await fetch(`/TeacherDashboard/GetRecords?courseId=${courseId}&from=${from}&to=${to}`);
        const data = await res.json();

        if (!data.length) {
            tbody.innerHTML = `<tr><td colspan="5" style="text-align:center;color:var(--muted);padding:24px;">No records found for this filter.</td></tr>`;
            return;
        }

        tbody.innerHTML = data.map(r => {
            const badgeClass = r.status === 'Present' ? 'badge-present'
                : r.status === 'Absent' ? 'badge-absent'
                    : 'badge-late';
            const dateStr = r.date
                ? new Date(r.date).toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' })
                : '—';
            return `<tr>
                <td>${dateStr}</td>
                <td class="mono">${r.studentNo}</td>
                <td>${r.studentName}</td>
                <td><span class="badge ${badgeClass}">${r.status}</span></td>
                <td style="color:var(--muted)">${r.remarks || '—'}</td>
            </tr>`;
        }).join('');

    } catch {
        tbody.innerHTML = `<tr><td colspan="5" style="text-align:center;color:#ef4444;padding:24px;">Failed to load records. Check connection.</td></tr>`;
    }
}

// ── EXPORT CSV ────────────────────────────────────────────
function exportCSV() {
    const rows = document.querySelectorAll('#recordsBody tr');
    if (!rows.length) { showToast('No data to export.', 'error'); return; }

    let csv = 'Date,Student No.,Name,Status,Remarks\n';
    rows.forEach(r => {
        const cells = r.querySelectorAll('td');
        if (cells.length >= 5) {
            csv += [...cells].map(c => `"${c.textContent.trim()}"`).join(',') + '\n';
        }
    });

    const a = document.createElement('a');
    a.href = 'data:text/csv;charset=utf-8,' + encodeURIComponent(csv);
    a.download = 'attendance_records.csv';
    a.click();
    showToast('CSV exported!', 'success');
}

// ── CLIENT-SIDE SEARCH ────────────────────────────────────
function filterStudents() {
    const q = (document.getElementById('searchInput') || document.getElementById('studentSearch'))?.value.toLowerCase() ?? '';
    const section = document.getElementById('sectionFilter')?.value ?? '';
    const rows = document.querySelectorAll('#studentBody tr');
    let n = 1;
    rows.forEach(row => {
        const matchQ = !q || (row.dataset.name ?? '').includes(q);
        const matchS = !section || (row.dataset.section ?? '') === section;
        row.style.display = (matchQ && matchS) ? '' : 'none';
        if (matchQ && matchS) {
            const nc = row.querySelector('.num-col');
            if (nc) nc.textContent = n++;
        }
    });
}

function filterTeachers() {
    const q = document.getElementById('teacherSearch')?.value.toLowerCase() ?? '';
    document.querySelectorAll('#teacherBody tr').forEach(row => {
        row.style.display = !q || (row.dataset.name ?? '').includes(q) ? '' : 'none';
    });
}

// ── DATA-* BUTTON HELPERS (fixes TS1109 Razor warning) ────
function openEditStudentFromBtn(btn) {
    openEditStudent(
        btn.dataset.id,
        btn.dataset.no,
        btn.dataset.first,
        btn.dataset.mid,
        btn.dataset.last,
        btn.dataset.email,
        btn.dataset.section,
        btn.dataset.mobile
    );
}

function confirmDeleteStudentFromBtn(btn) {
    confirmDeleteStudent(btn.dataset.id, btn.dataset.name);
}

function openEditTeacherFromBtn(btn) {
    openEditTeacher(
        btn.dataset.id,
        btn.dataset.no,
        btn.dataset.first,
        btn.dataset.last,
        btn.dataset.email
    );
}

function openEditCourseFromBtn(btn) {
    openEditCourse(
        btn.dataset.id,
        btn.dataset.code,
        btn.dataset.name,
        btn.dataset.units,
        btn.dataset.section,
        btn.dataset.schedule,
        btn.dataset.teacherid
    );
}

// ── EDIT STUDENT MODAL ────────────────────────────────────
function openEditStudent(id, no, first, mid, last, email, section, mobile) {
    document.getElementById('esId').value = id;
    document.getElementById('esNo').value = no;
    document.getElementById('esFirst').value = first;
    document.getElementById('esMid').value = mid;
    document.getElementById('esLast').value = last;
    document.getElementById('esEmail').value = email;
    document.getElementById('esSection').value = section;
    document.getElementById('esMobile').value = mobile;

    document.getElementById('editStudentNo').value = no;
    document.getElementById('editStudentFirst').value = first;
    document.getElementById('editStudentMid').value = mid;
    document.getElementById('editStudentLast').value = last;
    document.getElementById('editStudentEmail').value = email;
    document.getElementById('editStudentSec').value = section;
    document.getElementById('editStudentMobile').value = mobile;
    openModal('editStudentModal');
}

function saveEditStudent() {
    document.getElementById('esFirst').value = document.getElementById('editStudentFirst').value;
    document.getElementById('esMid').value = document.getElementById('editStudentMid').value;
    document.getElementById('esLast').value = document.getElementById('editStudentLast').value;
    document.getElementById('esEmail').value = document.getElementById('editStudentEmail').value;
    document.getElementById('esSection').value = document.getElementById('editStudentSec').value;
    document.getElementById('esMobile').value = document.getElementById('editStudentMobile').value;
    document.getElementById('editStudentForm').submit();
}

// ── DELETE STUDENT MODAL ──────────────────────────────────
function confirmDeleteStudent(id, name) {
    document.getElementById('dsId').value = id;
    document.getElementById('deleteStudentName').textContent = name;
    openModal('deleteStudentModal');
}

function submitDeleteStudent() {
    document.getElementById('deleteStudentForm').submit();
}

// ── EDIT TEACHER MODAL ────────────────────────────────────
function openEditTeacher(id, no, first, last, email) {
    document.getElementById('etId').value = id;
    document.getElementById('etNo').value = no;
    document.getElementById('etFirst').value = first;
    document.getElementById('etLast').value = last;
    document.getElementById('etEmail').value = email;

    document.getElementById('editTeacherId').value = no;
    document.getElementById('editTeacherFirst').value = first;
    document.getElementById('editTeacherLast').value = last;
    document.getElementById('editTeacherEmail').value = email;
    openModal('editTeacherModal');
}

function saveEditTeacher() {
    document.getElementById('etFirst').value = document.getElementById('editTeacherFirst').value;
    document.getElementById('etLast').value = document.getElementById('editTeacherLast').value;
    document.getElementById('etEmail').value = document.getElementById('editTeacherEmail').value;
    document.getElementById('editTeacherForm').submit();
}

// ── EDIT COURSE MODAL ─────────────────────────────────────
function openEditCourse(id, code, name, units, section, schedule, teacherId) {
    document.getElementById('ecId').value = id;
    document.getElementById('ecCode').value = code;
    document.getElementById('ecName').value = name;
    document.getElementById('ecUnits').value = units;
    document.getElementById('ecSection').value = section;
    document.getElementById('ecSchedule').value = schedule;
    document.getElementById('ecTeacherId').value = teacherId;
    openModal('editCourseModal');
}

function saveEditCourse() {
    document.getElementById('ecCodeHidden').value = document.getElementById('ecCode').value;
    document.getElementById('ecNameHidden').value = document.getElementById('ecName').value;
    document.getElementById('ecUnitsHidden').value = document.getElementById('ecUnits').value;
    document.getElementById('ecSectionHidden').value = document.getElementById('ecSection').value;
    document.getElementById('ecScheduleHidden').value = document.getElementById('ecSchedule').value;
    document.getElementById('ecTeacherIdHidden').value = document.getElementById('ecTeacherId').value;
    document.getElementById('editCourseForm').submit();
}

// ── QUICK REGISTER (Teacher Attendance page) ──────────────
async function quickRegister() {
    const studentNo = document.getElementById('qrStudentNo')?.value.trim();
    const firstName = document.getElementById('qrFirst')?.value.trim();
    const middleName = document.getElementById('qrMid')?.value.trim() ?? '';
    const lastName = document.getElementById('qrLast')?.value.trim();
    const section = document.getElementById('qrSection')?.value;
    const email = document.getElementById('qrEmail')?.value.trim() ?? '';
    const mobileNo = document.getElementById('qrMobile')?.value.trim() ?? '';

    if (!studentNo || !firstName || !lastName) {
        showToast('Student No., First Name and Last Name are required.', 'error');
        return;
    }

    // Read the antiforgery token rendered by Razor in #csrfForm
    const token = document.querySelector('#csrfForm input[name="__RequestVerificationToken"]')?.value ?? '';

    try {
        const formData = new URLSearchParams({
            __RequestVerificationToken: token,
            studentNo, firstName, middleName, lastName,
            email, section, mobileNo
        });

        const res = await fetch('/AdminDashboard/AddStudent', {
            method: 'POST',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
            body: formData
        });

        if (res.ok || res.redirected) {
            showToast(`${lastName}, ${firstName} registered successfully!`, 'success');
            ['qrStudentNo', 'qrFirst', 'qrMid', 'qrLast', 'qrEmail', 'qrMobile'].forEach(id => {
                const el = document.getElementById(id);
                if (el) el.value = '';
            });
        } else {
            showToast('Registration failed. Try again.', 'error');
        }
    } catch {
        showToast('Cannot reach server.', 'error');
    }
}

// ── ON PAGE READY ─────────────────────────────────────────
document.addEventListener('DOMContentLoaded', () => {
    filterStudentsByCourse();
});

// ── EDIT ACCOUNT MODAL ────────────────────────────────────
function openEditAccountFromBtn(btn) {
    document.getElementById('eaId').value = btn.dataset.id;
    document.getElementById('currentUsername').value = btn.dataset.username || '';
    document.getElementById('newUsername').value = '';
    document.getElementById('newPassword').value = '';
    openModal('editAccountModal');
}

function saveEditAccount() {
    document.getElementById('eaUsername').value = document.getElementById('newUsername').value;
    document.getElementById('eaPassword').value = document.getElementById('newPassword').value;
    document.getElementById('editAccountForm').submit();
}

// ── DELETE TEACHER MODAL ──────────────────────────────────
function confirmDeleteTeacherFromBtn(btn) {
    document.getElementById('dtId').value = btn.dataset.id;
    document.getElementById('deleteTeacherName').textContent = btn.dataset.name;
    openModal('deleteTeacherModal');
}

function submitDeleteTeacher() {
    document.getElementById('deleteTeacherForm').submit();
}
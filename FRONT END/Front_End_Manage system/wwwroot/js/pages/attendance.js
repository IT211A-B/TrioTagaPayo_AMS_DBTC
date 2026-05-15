// attendance.js - Attendance page functionality

document.addEventListener('DOMContentLoaded', function () {
    initAttendanceFilters();
    initStudentSearch();
});

function initAttendanceFilters() {
    const applyBtn = document.getElementById('applyFilter');
    if (!applyBtn) return;

    applyBtn.addEventListener('click', async function () {
        const courseId = document.getElementById('courseFilter')?.value;
        const status = document.getElementById('statusFilter')?.value;
        const fromDate = document.getElementById('fromDate')?.value;
        const toDate = document.getElementById('toDate')?.value;

        let url = '/Admin/AttendanceFilter?';
        if (courseId && courseId !== '') url += 'courseId=' + courseId + '&';
        if (status && status !== '') url += 'status=' + status + '&';
        if (fromDate) url += 'from=' + fromDate + '&';
        if (toDate) url += 'to=' + toDate;

        try {
            const response = await fetch(url);
            const html = await response.text();
            const tbody = document.getElementById('attendanceBody');
            if (tbody) tbody.innerHTML = html;
            updateSummaryCounts();
            if (typeof Toast !== 'undefined') Toast.success('Filter applied');
        } catch (error) {
            console.error('Error loading attendance:', error);
            if (typeof Toast !== 'undefined') Toast.error('Failed to load attendance records');
        }
    });
}

function initStudentSearch() {
    const searchInput = document.getElementById('studentSearch');
    if (!searchInput) return;

    searchInput.addEventListener('input', function () {
        const searchTerm = this.value.toLowerCase();
        const rows = document.querySelectorAll('#attendanceBody tr.table-row');
        let visibleCount = 0;
        rows.forEach(row => {
            const text = row.textContent.toLowerCase();
            const isVisible = text.includes(searchTerm);
            row.style.display = isVisible ? '' : 'none';
            if (isVisible) visibleCount++;
        });
        updatePagination(visibleCount);
    });
}

function updateSummaryCounts() {
    const rows = document.querySelectorAll('#attendanceBody tr.table-row');
    let total = 0, present = 0, absent = 0, late = 0;
    rows.forEach(row => {
        if (row.style.display === 'none') return;
        total++;
        const statusCell = row.querySelector('td:nth-child(5) .status-badge');
        if (statusCell) {
            const status = statusCell.textContent.trim();
            if (status.includes('Present')) present++;
            else if (status.includes('Absent')) absent++;
            else if (status.includes('Late')) late++;
        }
    });
    const totalSpan = document.getElementById('totalCount');
    const presentSpan = document.getElementById('presentCount');
    const absentSpan = document.getElementById('absentCount');
    const lateSpan = document.getElementById('lateCount');
    if (totalSpan) totalSpan.textContent = total;
    if (presentSpan) presentSpan.textContent = present;
    if (absentSpan) absentSpan.textContent = absent;
    if (lateSpan) lateSpan.textContent = late;
    updatePagination(total);
}

function updatePagination(visibleCount) {
    const startSpan = document.getElementById('showingStart');
    const endSpan = document.getElementById('showingEnd');
    const totalSpan = document.getElementById('totalRecords');
    if (startSpan) startSpan.innerText = visibleCount > 0 ? '1' : '0';
    if (endSpan) endSpan.innerText = visibleCount;
    if (totalSpan) totalSpan.innerText = visibleCount;
}
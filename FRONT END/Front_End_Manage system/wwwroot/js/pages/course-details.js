// course-details.js - Uses global openModal/closeModal from app.js

var pendingChanges = {};
var antiForgeryToken = '';

document.addEventListener('DOMContentLoaded', function () {
    var tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
    if (tokenInput) antiForgeryToken = tokenInput.value;

    var courseData = window.courseDetailsData;
    if (!courseData) {
        console.error('Course data not found');
        return;
    }

    setupEventListeners(courseData.courseId);
    setupAttendanceTableListeners();
});

function setupEventListeners(courseId) {
    var dateInput = document.getElementById('attendanceDate');
    if (dateInput) {
        dateInput.addEventListener('change', function () {
            loadAttendance(courseId, this.value);
        });
    }

    var markAllBtn = document.getElementById('markAllPresentBtn');
    if (markAllBtn) {
        markAllBtn.addEventListener('click', function () {
            markAllPresent(courseId);
        });
    }

    var generateQRBtn = document.getElementById('generateQRBtn');
    if (generateQRBtn) {
        generateQRBtn.addEventListener('click', function () {
            generateEnrollmentQR(courseId);
        });
    }

    // Modal close handlers
    var closeModalBtn = document.getElementById('closeModalBtn');
    if (closeModalBtn) {
        closeModalBtn.addEventListener('click', function () {
            if (typeof closeModal === 'function') closeModal('qrModal');
        });
    }

    var closeQrModalBtn = document.getElementById('closeQrModalBtn');
    if (closeQrModalBtn) {
        closeQrModalBtn.addEventListener('click', function () {
            if (typeof closeModal === 'function') closeModal('qrModal');
        });
    }

    var modalOverlay = document.getElementById('qrModal');
    if (modalOverlay) {
        modalOverlay.addEventListener('click', function (e) {
            if (e.target === this && typeof closeModal === 'function') closeModal('qrModal');
        });
    }

    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape' && typeof closeModal === 'function') closeModal('qrModal');
    });
}

function setupAttendanceTableListeners() {
    var statusSelects = document.querySelectorAll('.status-select');
    for (var i = 0; i < statusSelects.length; i++) {
        statusSelects[i].addEventListener('change', function () {
            var studentId = this.getAttribute('data-studentid');
            var status = this.value;
            if (!pendingChanges[studentId]) pendingChanges[studentId] = {};
            pendingChanges[studentId].status = status;
        });
    }

    var remarksInputs = document.querySelectorAll('.remarks-input');
    for (var i = 0; i < remarksInputs.length; i++) {
        remarksInputs[i].addEventListener('change', function () {
            var studentId = this.getAttribute('data-studentid');
            var remarks = this.value;
            if (!pendingChanges[studentId]) pendingChanges[studentId] = {};
            pendingChanges[studentId].remarks = remarks;
        });
    }

    var saveBtns = document.querySelectorAll('.btn-save');
    for (var i = 0; i < saveBtns.length; i++) {
        saveBtns[i].addEventListener('click', function () {
            var studentId = this.getAttribute('data-studentid');
            saveAttendance(studentId);
        });
    }
}

function loadAttendance(courseId, date) {
    window.location.href = '/Admin/CourseDetails/' + courseId + '?date=' + date;
}

async function saveAttendance(studentId) {
    var change = pendingChanges[studentId];
    if (!change) {
        if (typeof Toast !== 'undefined') Toast.info('No changes to save');
        return;
    }

    var btn = document.querySelector('.btn-save[data-studentid="' + studentId + '"]');
    var originalText = btn ? btn.textContent : 'Save';
    if (btn) {
        btn.disabled = true;
        btn.textContent = 'Saving...';
    }

    try {
        var date = document.getElementById('attendanceDate').value;
        var courseData = window.courseDetailsData;
        var courseId = courseData ? courseData.courseId : null;

        var response = await fetch('/api/Attendance/student/' + studentId);
        var records = await response.json();

        var existingRecord = null;
        for (var i = 0; i < records.length; i++) {
            if (records[i].courseId === courseId && records[i].date === date) {
                existingRecord = records[i];
                break;
            }
        }

        var result;
        if (existingRecord) {
            var formData = new URLSearchParams();
            formData.append('__RequestVerificationToken', antiForgeryToken);
            formData.append('attendanceId', existingRecord.id);
            formData.append('status', change.status || existingRecord.status);
            formData.append('remarks', change.remarks || existingRecord.remarks);

            result = await fetch('/Admin/UpdateAttendance', {
                method: 'POST',
                headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
                body: formData
            });
        } else {
            result = await fetch('/api/Attendance', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    studentId: parseInt(studentId),
                    courseId: courseId,
                    date: date,
                    status: change.status || 'Present',
                    remarks: change.remarks || ''
                })
            });
        }

        var data = await result.json();
        if (data.success) {
            if (typeof Toast !== 'undefined') Toast.success('Attendance saved');
            delete pendingChanges[studentId];
            setTimeout(function () { loadAttendance(courseId, date); }, 500);
        } else {
            if (typeof Toast !== 'undefined') Toast.error(data.message || 'Failed to save');
        }
    } catch (error) {
        console.error('Error saving attendance:', error);
        if (typeof Toast !== 'undefined') Toast.error('Error saving attendance');
    } finally {
        if (btn) {
            btn.disabled = false;
            btn.textContent = originalText;
        }
    }
}

async function markAllPresent(courseId) {
    var date = document.getElementById('attendanceDate').value;
    if (!confirm('Mark all students as present for ' + date + '?')) return;

    try {
        var formData = new URLSearchParams();
        formData.append('__RequestVerificationToken', antiForgeryToken);
        formData.append('courseId', courseId);
        formData.append('date', date);

        var response = await fetch('/Admin/MarkAllPresent', {
            method: 'POST',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
            body: formData
        });
        var data = await response.json();
        if (data.success) {
            if (typeof Toast !== 'undefined') Toast.success(data.message);
            loadAttendance(courseId, date);
        } else {
            if (typeof Toast !== 'undefined') Toast.error(data.message);
        }
    } catch (error) {
        console.error('Error marking all present:', error);
        if (typeof Toast !== 'undefined') Toast.error('Error marking all present');
    }
}

async function generateEnrollmentQR(courseId) {
    if (typeof openModal === 'function') {
        openModal('qrModal');
    } else {
        console.error('openModal not defined');
        return;
    }

    var qrContainer = document.getElementById('qrImageContainer');
    if (qrContainer) {
        qrContainer.innerHTML = '<div class="spinner-ring"></div>';
    }

    try {
        // FIXED: Using GET request to Admin endpoint with relative URL
        var response = await fetch('/Admin/GenerateCourseQRCode?courseId=' + courseId);
        var data = await response.json();

        if (data.success && data.qrCode) {
            if (qrContainer) {
                qrContainer.innerHTML = '<img src="data:image/png;base64,' + data.qrCode + '" style="width: 200px; height: 200px; margin: 0 auto; display: block;" />';
            }
            var courseNameElement = document.getElementById('qrCourseName');
            if (courseNameElement && data.courseName) {
                courseNameElement.textContent = data.courseName;
            }
        } else {
            if (qrContainer) {
                qrContainer.innerHTML = '<p style="color: #ef4444; text-align: center;">' + (data.message || 'Failed to generate QR code') + '</p>';
            }
            if (typeof Toast !== 'undefined') Toast.error(data.message || 'Failed to generate QR code');
        }
    } catch (error) {
        console.error('Error generating QR:', error);
        if (qrContainer) {
            qrContainer.innerHTML = '<p style="color: #ef4444; text-align: center;">Error generating QR code</p>';
        }
        if (typeof Toast !== 'undefined') Toast.error('Error generating QR code');
    }
}
// my-courses.js - Modal functionality for My Courses page

var antiForgeryToken = '';

document.addEventListener('DOMContentLoaded', function () {
    var tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
    if (tokenInput) antiForgeryToken = tokenInput.value;

    var modalOverlay = document.getElementById('qrModal');
    if (modalOverlay) {
        modalOverlay.addEventListener('click', function (e) {
            if (e.target === this && typeof closeModal === 'function') {
                closeModal('qrModal');
            }
        });
    }

    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape' && typeof closeModal === 'function') {
            closeModal('qrModal');
        }
    });

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
});

// Attendance QR (new) – use this for daily attendance
async function generateAttendanceQR(courseId, courseName) {
    if (typeof openModal !== 'function') {
        console.error('openModal not defined');
        return;
    }
    openModal('qrModal');

    var qrContainer = document.getElementById('qrImageContainer');
    if (qrContainer) {
        qrContainer.innerHTML = '<div class="spinner-ring"></div>';
    }

    var courseNameElement = document.getElementById('qrCourseName');
    if (courseNameElement) {
        courseNameElement.textContent = courseName;
    }

    const today = new Date().toISOString().slice(0, 10);
    const formData = new URLSearchParams();
    formData.append('__RequestVerificationToken', antiForgeryToken);
    formData.append('courseId', courseId);
    formData.append('date', today);

    try {
        const response = await fetch('/Teacher/GenerateAttendanceQR', {
            method: 'POST',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
            body: formData
        });
        const data = await response.json();

        if (data.success && data.qrCode) {
            if (qrContainer) {
                qrContainer.innerHTML = '<img src="data:image/png;base64,' + data.qrCode + '" style="width: 200px; height: 200px; margin: 0 auto; display: block;" />';
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

// Enrollment QR (kept for student self-enrollment)
async function generateEnrollmentQR(courseId, courseName) {
    if (typeof openModal !== 'function') {
        console.error('openModal not defined');
        return;
    }
    openModal('qrModal');

    var qrContainer = document.getElementById('qrImageContainer');
    if (qrContainer) {
        qrContainer.innerHTML = '<div class="spinner-ring"></div>';
    }

    var courseNameElement = document.getElementById('qrCourseName');
    if (courseNameElement) {
        courseNameElement.textContent = courseName;
    }

    const formData = new URLSearchParams();
    formData.append('__RequestVerificationToken', antiForgeryToken);
    formData.append('courseId', courseId);

    try {
        const response = await fetch('/Teacher/GenerateCourseQRCode', {
            method: 'POST',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
            body: formData
        });
        const data = await response.json();

        if (data.success && data.qrCode) {
            if (qrContainer) {
                qrContainer.innerHTML = '<img src="data:image/png;base64,' + data.qrCode + '" style="width: 200px; height: 200px; margin: 0 auto; display: block;" />';
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
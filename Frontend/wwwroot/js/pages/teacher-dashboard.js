// teacher-dashboard.js

async function generateEnrollmentQR(courseId, courseName) {
    var qrContainer = document.getElementById('qrImageContainer');
    var courseNameSpan = document.getElementById('qrCourseName');

    if (courseNameSpan) courseNameSpan.textContent = courseName;
    if (qrContainer) qrContainer.innerHTML = '<div class="spinner-ring"></div>';

    openModal('qrModal');

    var token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
    var formData = new URLSearchParams();
    formData.append('courseId', courseId);
    if (token) formData.append('__RequestVerificationToken', token);

    try {
        var response = await fetch('/Teacher/GenerateCourseQRCode', {
            method: 'POST',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
            body: formData
        });
        var data = await response.json();

        if (data.success && data.qrCode) {
            if (qrContainer) {
                qrContainer.innerHTML = '<img src="data:image/png;base64,' + data.qrCode + '" style="width: 180px; height: 180px; margin: 0 auto; display: block;" />';
            }
        } else {
            if (qrContainer) {
                qrContainer.innerHTML = '<p style="color: #ef4444; text-align: center;">Failed to generate QR code</p>';
            }
            if (typeof Toast !== 'undefined') Toast.error(data.message || 'Failed to generate QR code');
        }
    } catch (error) {
        if (qrContainer) {
            qrContainer.innerHTML = '<p style="color: #ef4444; text-align: center;">Error generating QR code</p>';
        }
        if (typeof Toast !== 'undefined') Toast.error('An unexpected error occurred');
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

document.addEventListener('keydown', function (e) {
    if (e.key === 'Escape') {
        document.querySelectorAll('.modal-overlay.active').forEach(function (m) {
            m.classList.remove('active');
        });
    }
});

document.querySelectorAll('.modal-overlay').forEach(function (overlay) {
    overlay.addEventListener('click', function (e) {
        if (e.target === this) this.classList.remove('active');
    });
});
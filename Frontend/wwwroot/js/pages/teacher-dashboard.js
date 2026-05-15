// teacher-dashboard.js

async function generateEnrollmentQR(courseId, courseName) {
    var qrContainer = document.getElementById('qrImageContainer');
    var courseNameSpan = document.getElementById('qrCourseName');

    if (courseNameSpan) courseNameSpan.textContent = courseName;
    if (qrContainer) qrContainer.innerHTML = '<div class="spinner-ring"></div>';

    // Open modal
    if (typeof openModal === 'function') {
        openModal('qrModal');
    } else {
        console.error('openModal function not found');
        return;
    }

    try {
        // FIXED: Use GET request to Admin endpoint (relative URL)
        var response = await fetch('/Admin/GenerateCourseQRCode?courseId=' + courseId);
        var data = await response.json();

        console.log('QR Response:', data);

        if (data.success && data.qrCode) {
            if (qrContainer) {
                var qrCodeValue = data.qrCode;
                if (typeof qrCodeValue === 'object') {
                    qrCodeValue = qrCodeValue.qrCode || JSON.stringify(qrCodeValue);
                }
                qrContainer.innerHTML = '<img src="data:image/png;base64,' + qrCodeValue + '" style="width: 180px; height: 180px; margin: 0 auto; display: block;" />';
            }
        } else {
            var errorMsg = data.message || 'Failed to generate QR code';
            if (qrContainer) {
                qrContainer.innerHTML = '<p style="color: #ef4444; text-align: center;">Error: ' + errorMsg + '</p>';
            }
            if (typeof Toast !== 'undefined') Toast.error(errorMsg);
        }
    } catch (error) {
        console.error('QR Error:', error);
        if (qrContainer) {
            qrContainer.innerHTML = '<p style="color: #ef4444; text-align: center;">Error: ' + error.message + '</p>';
        }
        if (typeof Toast !== 'undefined') Toast.error('An unexpected error occurred');
    }
}
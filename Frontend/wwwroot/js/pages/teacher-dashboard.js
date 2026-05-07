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

        // Log the full response to console
        console.log('QR Response:', data);

        if (data.success && data.qrCode) {
            if (qrContainer) {
                qrContainer.innerHTML = '<img src="data:image/png;base64,' + data.qrCode + '" style="width: 180px; height: 180px; margin: 0 auto; display: block;" />';
            }
        } else {
            // Show the actual error message
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
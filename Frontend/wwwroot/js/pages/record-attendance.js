(function () {
    'use strict';

    const form = document.getElementById('attendanceForm');
    const submitBtn = document.getElementById('submitBtn');
    const messageDiv = document.getElementById('message');

    if (!form) {
        console.error('Attendance form not found');
        return;
    }

    async function submitAttendance(formData) {
        const token = document.querySelector('input[name="__RequestVerificationToken"]').value;

        const payload = new URLSearchParams();
        payload.append('__RequestVerificationToken', token);
        payload.append('courseId', formData.get('courseId'));
        payload.append('date', formData.get('date'));
        payload.append('studentId', formData.get('studentId'));
        payload.append('studentName', formData.get('studentName'));

        const response = await fetch('/Student/RecordAttendance', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded'
            },
            body: payload
        });

        return await response.json();
    }

    function showMessage(type, text) {
        if (!messageDiv) return;

        messageDiv.className = 'message ' + type;
        messageDiv.textContent = (type === 'success' ? '✅ ' : '❌ ') + text;
    }

    function hideMessage() {
        if (messageDiv) {
            messageDiv.className = 'message';
            messageDiv.style.display = 'none';
        }
    }

    function setButtonLoading(isLoading, originalText) {
        if (!submitBtn) return;

        submitBtn.disabled = isLoading;

        if (isLoading) {
            submitBtn.textContent = 'PROCESSING...';
        } else {
            submitBtn.textContent = originalText || '✓ MARK ATTENDANCE';
        }
    }

    async function handleSubmit(event) {
        event.preventDefault();

        hideMessage();

        const studentId = document.getElementById('studentId')?.value.trim();
        const studentName = document.getElementById('studentName')?.value.trim();

        if (!studentId || !studentName) {
            showMessage('error', 'Please fill in both Student ID and Full Name');
            return;
        }

        const originalButtonText = submitBtn?.textContent;
        setButtonLoading(true, originalButtonText);

        try {
            const formData = new FormData(form);
            const data = await submitAttendance(formData);

            if (data.success) {
                showMessage('success', data.message || 'Attendance recorded successfully!');
                setButtonLoading(false, originalButtonText);

                // Redirect to login page after 2 seconds
                setTimeout(function () {
                    window.location.href = '/Account/StudentLogin';
                }, 2000);
            } else {
                showMessage('error', data.message || 'Failed to record attendance');
                setButtonLoading(false, originalButtonText);
            }
        } catch (error) {
            console.error('Attendance submission error:', error);
            showMessage('error', 'Network error. Please try again.');
            setButtonLoading(false, originalButtonText);
        }
    }

    // Add event listener
    form.addEventListener('submit', handleSubmit);

    // Auto-focus on first input
    const firstInput = document.getElementById('studentId');
    if (firstInput) {
        firstInput.focus();
    }
})();
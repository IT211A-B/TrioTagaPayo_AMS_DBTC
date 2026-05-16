(function () {
    'use strict';

    var form = document.getElementById('attendanceForm');
    var submitBtn = document.getElementById('submitBtn');
    var messageDiv = document.getElementById('message');

    if (!form) {
        console.error('Attendance form not found');
        return;
    }

    async function submitAttendance(formData) {
        var token = document.querySelector('input[name="__RequestVerificationToken"]').value;

        var payload = new URLSearchParams();
        payload.append('__RequestVerificationToken', token);
        payload.append('courseId', formData.get('courseId'));
        payload.append('date', formData.get('date'));
        payload.append('studentId', formData.get('studentId'));
        payload.append('studentName', formData.get('studentName'));

        var response = await fetch('/Student/RecordAttendance', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded'
            },
            body: payload
        });

        return await response.json();
    }

    function showMessage(type, text) {
        if (!messageDiv) {
            return;
        }

        messageDiv.className = 'message ' + type;
        messageDiv.textContent = (type === 'success' ? '✅ ' : '❌ ') + text;

        if (type === 'success') {
            setTimeout(function () {
                if (messageDiv) {
                    messageDiv.style.display = 'none';
                }
            }, 10000);
        }
    }

    function hideMessage() {
        if (messageDiv) {
            messageDiv.className = 'message';
            messageDiv.style.display = 'none';
        }
    }

    function setButtonLoading(isLoading, originalText) {
        if (!submitBtn) {
            return;
        }

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

        var studentId = document.getElementById('studentId');
        var studentName = document.getElementById('studentName');

        var studentIdValue = studentId ? studentId.value.trim() : '';
        var studentNameValue = studentName ? studentName.value.trim() : '';

        if (!studentIdValue || !studentNameValue) {
            showMessage('error', 'Please fill in both Student ID and Full Name');
            return;
        }

        var originalButtonText = submitBtn ? submitBtn.textContent : '';
        setButtonLoading(true, originalButtonText);

        try {
            var formData = new FormData(form);
            var data = await submitAttendance(formData);

            if (data.success) {
                showMessage('success', data.message || 'Attendance recorded successfully!');
                setButtonLoading(false, originalButtonText);

                setTimeout(function () {
                    window.location.href = '/Student/Scanner';
                }, 3000);
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

    form.addEventListener('submit', handleSubmit);

    var firstInput = document.getElementById('studentId');
    if (firstInput) {
        firstInput.focus();
    }
})();
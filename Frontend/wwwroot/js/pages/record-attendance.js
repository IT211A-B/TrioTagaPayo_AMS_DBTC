(function () {
    'use strict';

    var form = document.getElementById('attendanceForm');
    var submitBtn = document.getElementById('submitBtn');
    var messageDiv = document.getElementById('message');

    if (form === null) {
        console.error('Attendance form not found');
        return;
    }

    async function submitAttendance(formData) {
        var tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
        var token = '';

        if (tokenInput !== null) {
            token = tokenInput.value;
        }

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
        if (messageDiv === null) {
            return;
        }

        messageDiv.className = 'message ' + type;
        messageDiv.textContent = (type === 'success' ? '✅ ' : '❌ ') + text;

        if (type === 'success') {
            setTimeout(function () {
                if (messageDiv !== null) {
                    messageDiv.style.display = 'none';
                }
            }, 10000);
        }
    }

    function hideMessage() {
        if (messageDiv !== null) {
            messageDiv.className = 'message';
            messageDiv.style.display = 'none';
        }
    }

    function setButtonLoading(isLoading, originalText) {
        if (submitBtn === null) {
            return;
        }

        submitBtn.disabled = isLoading;

        if (isLoading) {
            submitBtn.textContent = 'PROCESSING...';
        } else {
            var buttonText = originalText || '✓ MARK ATTENDANCE';
            submitBtn.textContent = buttonText;
        }
    }

    async function handleSubmit(event) {
        event.preventDefault();

        hideMessage();

        var studentIdElement = document.getElementById('studentId');
        var studentNameElement = document.getElementById('studentName');

        var studentIdValue = '';
        var studentNameValue = '';

        if (studentIdElement !== null) {
            studentIdValue = studentIdElement.value.trim();
        }

        if (studentNameElement !== null) {
            studentNameValue = studentNameElement.value.trim();
        }

        if (studentIdValue === '' || studentNameValue === '') {
            showMessage('error', 'Please fill in both Student ID and Full Name');
            return;
        }

        var originalButtonText = '';
        if (submitBtn !== null) {
            originalButtonText = submitBtn.textContent;
        }
        setButtonLoading(true, originalButtonText);

        try {
            var formData = new FormData(form);
            var data = await submitAttendance(formData);

            if (data.success) {
                var successMsg = data.message || 'Attendance recorded successfully!';
                showMessage('success', successMsg);
                setButtonLoading(false, originalButtonText);

                setTimeout(function () {
                    window.location.href = '/Student/Scanner';
                }, 3000);
            } else {
                var errorMsg = data.message || 'Failed to record attendance';
                showMessage('error', errorMsg);
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
    if (firstInput !== null) {
        firstInput.focus();
    }
})();   
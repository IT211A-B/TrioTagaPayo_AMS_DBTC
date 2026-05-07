    // self-enroll.js - Student QR code self-enrollment
    // Uses correct API: POST /api/Enrollment/self-enroll

    const courseId = new URLSearchParams(window.location.search).get('courseId');

    // Load course info when page loads
    document.addEventListener('DOMContentLoaded', function () {
        if (!courseId) {
            showMessage('Invalid course link. Please scan a valid QR code.', 'error');
            return;
        }

        // Use the /api/Enrollment/course/{id} endpoint to get course details
        fetch('/api/Enrollment/course/' + courseId)
            .then(function (res) {
                if (!res.ok) throw new Error('Course not found');
                return res.json();
            })
            .then(function (data) {
                var courseInfo = document.getElementById('courseInfo');
                if (courseInfo && data) {
                    courseInfo.innerHTML = '<span style="color:#38bdf8;">' + (data.courseCode || '') + '</span><br>' +
                        '<span style="color:#e2eaf4; font-weight:600;">' + (data.courseName || 'Course') + '</span><br>' +
                        '<span style="font-size:11px; color:#64748b;">Section: ' + (data.section || 'N/A') + '</span>';
                }
            })
            .catch(function () {
                var courseInfo = document.getElementById('courseInfo');
                if (courseInfo) {
                    courseInfo.innerHTML = '<span style="color:#38bdf8;">Course ready</span>';
                }
            });

        // Add enter key listeners
        var studentIdInput = document.getElementById('studentId');
        var fullNameInput = document.getElementById('fullName');

        if (studentIdInput) {
            studentIdInput.addEventListener('keypress', function (e) {
                if (e.key === 'Enter') submitEnrollment();
            });
        }

        if (fullNameInput) {
            fullNameInput.addEventListener('keypress', function (e) {
                if (e.key === 'Enter') submitEnrollment();
            });
        }
    });

    // Submit enrollment function using correct API
    window.submitEnrollment = async function () {
        var studentId = document.getElementById('studentId').value.trim();
        var fullName = document.getElementById('fullName').value.trim();
        var btn = document.getElementById('enrollBtn');

        if (!studentId || !fullName) {
            showMessage('Please enter both Student ID and Full Name', 'error');
            return;
        }

        if (!courseId) {
            showMessage('Invalid course. Please scan a valid QR code.', 'error');
            return;
        }

        btn.disabled = true;
        btn.textContent = 'Enrolling...';

        try {
            // Use the correct endpoint from Swagger: POST /api/Enrollment/self-enroll
            var response = await fetch('/api/Enrollment/self-enroll', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    studentId: studentId,
                    fullName: fullName,
                    courseId: parseInt(courseId)
                })
            });

            var data = await response.json();

            if (data.success) {
                showMessage('✓ Successfully enrolled in course! This window will close in 3 seconds.', 'success');
                setTimeout(function () { window.close(); }, 3000);
            } else {
                showMessage(data.message || 'Enrollment failed. Please check your information.', 'error');
            }
        } catch (error) {
            console.error('Enrollment error:', error);
            showMessage('Connection error. Please try again.', 'error');
        } finally {
            btn.disabled = false;
            btn.textContent = 'Enroll Now';
        }
    };

    function showMessage(msg, type) {
        var messageArea = document.getElementById('messageArea');
        if (!messageArea) return;

        var className = (type === 'success') ? 'message success' : (type === 'error') ? 'message error' : 'message info';
        messageArea.innerHTML = '<div class="' + className + '">' + escapeHtml(msg) + '</div>';

        setTimeout(function () {
            if (messageArea.firstChild) messageArea.firstChild.remove();
        }, 5000);
    }

    function escapeHtml(str) {
        if (!str) return '';
        return str.replace(/[&<>]/g, function (m) {
            if (m === '&') return '&amp;';
            if (m === '<') return '&lt;';
            if (m === '>') return '&gt;';
            return m;
        });
    }
(function () {
    'use strict';

    var courseId = null;
    var isSubmitting = false;

    document.addEventListener('DOMContentLoaded', function () {
        var urlParams = new URLSearchParams(window.location.search);
        courseId = urlParams.get('courseId');

        if (!courseId) {
            showMessage('Invalid course link. Please scan a valid QR code.', 'error');
        } else {
            loadCourseInfo();
        }

        var enrollBtn = document.getElementById('enrollBtn');
        if (enrollBtn) {
            enrollBtn.addEventListener('click', submitEnrollment);
        }
    });

    async function loadCourseInfo() {
        var courseInfoDiv = document.getElementById('courseInfo');
        var enrollForm = document.getElementById('enrollForm');

        if (!courseInfoDiv) return;

        courseInfoDiv.innerHTML = '<div class="loading">Loading course information...</div>';

        try {
            var response = await fetch('/api/Course/' + courseId);

            if (!response.ok) {
                throw new Error('Course not found');
            }

            var data = await response.json();

            if (courseInfoDiv && data) {
                var courseCode = data.courseCode || '';
                var courseName = data.courseName || 'Course';
                var section = data.section || 'N/A';
                var schedule = data.schedule || 'N/A';
                var teacherName = data.teacherName || 'N/A';

                courseInfoDiv.innerHTML =
                    '<div class="course-detail">' +
                    '<span class="course-code">' + escapeHtml(courseCode) + '</span>' +
                    '<h3 class="course-name">' + escapeHtml(courseName) + '</h3>' +
                    '<p class="course-meta">Section: ' + escapeHtml(section) + ' | Schedule: ' + escapeHtml(schedule) + '</p>' +
                    '<p class="course-teacher">Teacher: ' + escapeHtml(teacherName) + '</p>' +
                    '</div>';

                if (enrollForm) {
                    enrollForm.style.display = 'block';
                }
            }
        } catch (error) {
            console.error('Error loading course:', error);
            if (courseInfoDiv) {
                courseInfoDiv.innerHTML = '<p class="error">Failed to load course information. Please try again.</p>';
            }
            showMessage('Could not load course details. Please check your connection.', 'error');
        }
    }

    async function submitEnrollment() {
        if (isSubmitting) return;

        var studentIdInput = document.getElementById('studentId');
        var fullNameInput = document.getElementById('fullName');
        var passwordInput = document.getElementById('password');
        var confirmPasswordInput = document.getElementById('confirmPassword');

        var studentId = studentIdInput ? studentIdInput.value.trim() : '';
        var fullName = fullNameInput ? fullNameInput.value.trim() : '';
        var password = passwordInput ? passwordInput.value : '';
        var confirmPassword = confirmPasswordInput ? confirmPasswordInput.value : '';

        var btn = document.getElementById('enrollBtn');

        if (!studentId || !fullName || !password || !confirmPassword) {
            showMessage('Please fill in all fields', 'error');
            return;
        }

        if (password !== confirmPassword) {
            showMessage('Passwords do not match', 'error');
            return;
        }

        if (password.length < 6) {
            showMessage('Password must be at least 6 characters', 'error');
            return;
        }

        if (!courseId) {
            showMessage('Invalid course. Please scan a valid QR code.', 'error');
            return;
        }

        isSubmitting = true;
        if (btn) {
            btn.disabled = true;
            btn.textContent = 'Creating Account...';
        }

        try {
            var registerResponse = await fetch('/api/Auth/register', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    username: studentId,
                    password: password,
                    fullName: fullName,
                    role: 'Student'
                })
            });

            var registerData = await registerResponse.json();

            if (!registerData.success) {
                var errorMsg = registerData.message || 'Account creation failed';
                if (errorMsg.includes('duplicate') || errorMsg.includes('already exists')) {
                    errorMsg = 'Student ID already exists. Please login instead.';
                    setTimeout(function () {
                        window.location.href = '/Account/StudentLogin';
                    }, 3000);
                }
                showMessage(errorMsg, 'error');
                isSubmitting = false;
                if (btn) {
                    btn.disabled = false;
                    btn.textContent = 'Create Account & Enroll';
                }
                return;
            }

            if (btn) {
                btn.textContent = 'Enrolling in Course...';
            }

            var enrollResponse = await fetch('/api/Enrollment/self-enroll', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    studentId: registerData.studentId || studentId,
                    fullName: fullName,
                    courseId: parseInt(courseId, 10)
                })
            });

            var enrollData = await enrollResponse.json();

            if (enrollData.success) {
                showMessage('✓ Account created and enrolled successfully! Redirecting to login...', 'success');
                setTimeout(function () {
                    window.location.href = '/Account/StudentLogin';
                }, 3000);
            } else {
                showMessage(enrollData.message || 'Enrollment failed, but account was created. Please contact your teacher.', 'warning');
                setTimeout(function () {
                    window.location.href = '/Account/StudentLogin';
                }, 4000);
            }
        } catch (error) {
            console.error('Enrollment error:', error);
            showMessage('Connection error. Please try again.', 'error');
            isSubmitting = false;
            if (btn) {
                btn.disabled = false;
                btn.textContent = 'Create Account & Enroll';
            }
        }
    }

    function showMessage(msg, type) {
        var messageArea = document.getElementById('messageArea');
        if (!messageArea) return;

        var className = '';
        if (type === 'success') {
            className = 'message success';
        } else if (type === 'error') {
            className = 'message error';
        } else {
            className = 'message warning';
        }

        messageArea.innerHTML = '<div class="' + className + '">' + escapeHtml(msg) + '</div>';
        setTimeout(function () {
            if (messageArea.firstChild) {
                messageArea.firstChild.remove();
            }
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
})();
// self-enroll.js - Student Self-Registration via QR Code
// Creates account AND enrolls student in one step

const courseId = new URLSearchParams(window.location.search).get('courseId');
let isSubmitting = false;

// Load course info when page loads
document.addEventListener('DOMContentLoaded', function () {
    if (!courseId) {
        showMessage('Invalid course link. Please scan a valid QR code.', 'error');
        return;
    }

    loadCourseInfo();
    setupEventListeners();
});

async function loadCourseInfo() {
    const courseInfo = document.getElementById('courseInfo');
    if (!courseInfo) return;

    courseInfo.innerHTML = '<div class="loading">Loading course information...</div>';

    try {
        const response = await fetch('/api/Enrollment/course/' + courseId);
        if (!response.ok) throw new Error('Course not found');
        const data = await response.json();

        if (courseInfo && data) {
            courseInfo.innerHTML = `
                <div class="course-detail">
                    <span class="course-code">${escapeHtml(data.courseCode || '')}</span>
                    <h3 class="course-name">${escapeHtml(data.courseName || 'Course')}</h3>
                    <p class="course-meta">Section: ${escapeHtml(data.section || 'N/A')} | Schedule: ${escapeHtml(data.schedule || 'N/A')}</p>
                    <p class="course-teacher">Teacher: ${escapeHtml(data.teacherName || 'N/A')}</p>
                </div>
            `;
        }
    } catch (error) {
        console.error('Error loading course:', error);
        if (courseInfo) {
            courseInfo.innerHTML = '<p class="course-ready">Course ready for enrollment</p>';
        }
    }
}

function setupEventListeners() {
    const studentIdInput = document.getElementById('studentId');
    const fullNameInput = document.getElementById('fullName');
    const passwordInput = document.getElementById('password');
    const confirmPasswordInput = document.getElementById('confirmPassword');
    const enrollBtn = document.getElementById('enrollBtn');

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

    if (passwordInput) {
        passwordInput.addEventListener('keypress', function (e) {
            if (e.key === 'Enter') submitEnrollment();
        });
    }

    if (confirmPasswordInput) {
        confirmPasswordInput.addEventListener('keypress', function (e) {
            if (e.key === 'Enter') submitEnrollment();
        });
    }
}

async function submitEnrollment() {
    if (isSubmitting) return;

    const studentId = document.getElementById('studentId').value.trim();
    const fullName = document.getElementById('fullName').value.trim();
    const password = document.getElementById('password').value;
    const confirmPassword = document.getElementById('confirmPassword').value;
    const btn = document.getElementById('enrollBtn');

    // Validation
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
    btn.disabled = true;
    btn.textContent = 'Creating Account...';

    try {
        // Step 1: Register the student account
        const registerResponse = await fetch('/api/Auth/register', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                username: studentId,
                password: password,
                fullName: fullName,
                role: 'Student'
            })
        });

        const registerData = await registerResponse.json();

        if (!registerData.success) {
            let errorMsg = registerData.message || 'Account creation failed';
            if (errorMsg.includes('duplicate') || errorMsg.includes('already exists')) {
                errorMsg = 'Student ID already exists. Please login instead.';
            }
            showMessage(errorMsg, 'error');
            isSubmitting = false;
            btn.disabled = false;
            btn.textContent = 'Create Account & Enroll';
            return;
        }

        btn.textContent = 'Enrolling in Course...';

        // Step 2: Enroll in the course
        const enrollResponse = await fetch('/api/Enrollment/self-enroll', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                studentId: registerData.studentId || studentId,
                fullName: fullName,
                courseId: parseInt(courseId)
            })
        });

        const enrollData = await enrollResponse.json();

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
        btn.disabled = false;
        btn.textContent = 'Create Account & Enroll';
    }
}

function showMessage(msg, type) {
    const messageArea = document.getElementById('messageArea');
    if (!messageArea) return;

    const className = type === 'success' ? 'message success' :
        type === 'error' ? 'message error' :
            'message warning';
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
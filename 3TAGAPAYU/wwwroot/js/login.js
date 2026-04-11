document.addEventListener('DOMContentLoaded', function () {
    const togglePasswordBtn = document.getElementById('togglePassword');
    const passwordInput = document.getElementById('password');
    const loginForm = document.getElementById('loginForm');

    // Toggle password visibility
    if (togglePasswordBtn) {
        togglePasswordBtn.addEventListener('click', function (e) {
            e.preventDefault();
            const type = passwordInput.type === 'password' ? 'text' : 'password';
            passwordInput.type = type;
            togglePasswordBtn.classList.toggle('active');
        });
    }

    // Form validation
    if (loginForm) {
        loginForm.addEventListener('submit', function (e) {
            if (!validateForm()) {
                e.preventDefault();
            }
        });
    }

    function validateForm() {
        const email = document.getElementById('email').value.trim();
        const password = document.getElementById('password').value.trim();

        if (!email) {
            showError('email', 'Email is required');
            return false;
        }

        if (!isValidEmail(email)) {
            showError('email', 'Invalid email format');
            return false;
        }

        if (!password) {
            showError('password', 'Password is required');
            return false;
        }

        if (password.length < 6) {
            showError('password', 'Password must be at least 6 characters');
            return false;
        }

        return true;
    }

    function isValidEmail(email) {
        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        return emailRegex.test(email);
    }

    function showError(fieldId, message) {
        const field = document.getElementById(fieldId);
        const errorElement = field.parentElement.querySelector('.error-message');
        if (errorElement) {
            errorElement.textContent = message;
            field.focus();
        }
    }

    // Clear error on input
    const inputs = document.querySelectorAll('.form-control');
    inputs.forEach(input => {
        input.addEventListener('input', function () {
            const errorElement = this.parentElement.querySelector('.error-message');
            if (errorElement) {
                errorElement.textContent = '';
            }
        });
    });
});
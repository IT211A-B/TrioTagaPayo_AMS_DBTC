// student-login.js

document.addEventListener('DOMContentLoaded', function () {
    var loginBtn = document.getElementById('loginBtn');

    if (loginBtn) {
        loginBtn.addEventListener('click', function () {
            this.disabled = true;
            this.textContent = 'LOGGING IN...';
        });
    }
});
document.addEventListener('DOMContentLoaded', function () {
    const adminBtn = document.querySelector('.admin-btn');
    const teacherBtn = document.querySelector('.teacher-btn');
    const userTypeInput = document.getElementById('userType');
    const usernameInput = document.getElementById('Username');
    const submitBtn = document.querySelector('.sign-in-btn');
    const footerText = document.querySelector('.footer-text');
        
    function setAdmin() {
        adminBtn.classList.add('active');
        teacherBtn.classList.remove('active');
        userTypeInput.value = 'Admin';
        usernameInput.placeholder = 'Enter admin username';
        submitBtn.textContent = 'Sign In as Admin →';
        footerText.textContent = 'Administrator access only';
    }

    function setTeacher() {
        teacherBtn.classList.add('active');
        adminBtn.classList.remove('active');
        userTypeInput.value = 'Teacher';
        usernameInput.placeholder = 'Enter teacher username';
        submitBtn.textContent = 'Sign In as Teacher →';
        footerText.textContent = 'Teacher access only';
    }

    adminBtn.addEventListener('click', setAdmin);
    teacherBtn.addEventListener('click', setTeacher);

    // Set default on load
    setAdmin();
});
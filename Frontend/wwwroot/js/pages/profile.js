// profile.js - Profile page functionality

document.addEventListener('DOMContentLoaded', function () {
    initProfilePhotoFeature();
    initProfileForm();
});

function initProfilePhotoFeature() {
    const profileWrapper = document.getElementById('profilePhotoWrapper');
    const profileImg = document.getElementById('profilePhotoImg');
    const photoModal = document.getElementById('photoModal');
    const modalPhotoPreview = document.getElementById('modalPhotoPreview');
    const fullImageViewer = document.getElementById('fullImageViewer');
    const fullSizeImage = document.getElementById('fullSizeImage');
    const fileInput = document.getElementById('photoFileInput');

    if (!profileWrapper) return;

    let currentPhotoUrl = profileImg ? profileImg.src : null;

    profileWrapper.addEventListener('click', function () {
        if (modalPhotoPreview) {
            modalPhotoPreview.src = currentPhotoUrl || '/images/default-avatar.png';
        }
        if (photoModal) photoModal.style.display = 'flex';
    });

    function closeModal() {
        if (photoModal) photoModal.style.display = 'none';
    }

    const closeModalBtn = document.getElementById('closeModal');
    const cancelPhotoBtn = document.getElementById('cancelPhotoBtn');
    if (closeModalBtn) closeModalBtn.addEventListener('click', closeModal);
    if (cancelPhotoBtn) cancelPhotoBtn.addEventListener('click', closeModal);

    window.addEventListener('click', function (event) {
        if (event.target === photoModal) closeModal();
        if (event.target === fullImageViewer) closeFullView();
    });

    const viewFullBtn = document.getElementById('viewFullBtn');
    if (viewFullBtn) {
        viewFullBtn.addEventListener('click', function () {
            if (fullSizeImage && modalPhotoPreview) {
                fullSizeImage.src = modalPhotoPreview.src;
            }
            if (fullImageViewer) fullImageViewer.style.display = 'flex';
            closeModal();
        });
    }

    function closeFullView() {
        if (fullImageViewer) fullImageViewer.style.display = 'none';
    }
    const closeFullViewBtn = document.getElementById('closeFullView');
    if (closeFullViewBtn) closeFullViewBtn.addEventListener('click', closeFullView);

    const changePhotoBtn = document.getElementById('changePhotoBtn');
    if (changePhotoBtn && fileInput) {
        changePhotoBtn.addEventListener('click', function () {
            fileInput.click();
            closeModal();
        });
    }

    if (fileInput) {
        fileInput.addEventListener('change', function (event) {
            const file = event.target.files[0];
            if (file) {
                const allowedTypes = ['image/jpeg', 'image/png', 'image/gif', 'image/webp'];
                if (!allowedTypes.includes(file.type)) {
                    showMessage('Only JPG, PNG, GIF, or WEBP images are allowed', 'error');
                    return;
                }
                if (file.size > 5 * 1024 * 1024) {
                    showMessage('File size must be less than 5MB', 'error');
                    return;
                }
                const reader = new FileReader();
                reader.onload = function (e) {
                    if (profileImg) {
                        profileImg.src = e.target.result;
                        currentPhotoUrl = e.target.result;
                    }
                    uploadPhotoToServer(file);
                };
                reader.readAsDataURL(file);
            }
        });
    }
}

function initProfileForm() {
    const form = document.getElementById('profileForm');
    const saveBtn = document.getElementById('saveBtn');
    const newPassword = document.getElementById('newPassword');
    const confirmPassword = document.getElementById('confirmPassword');

    if (!form) return;

    form.addEventListener('submit', function (e) {
        e.preventDefault();

        if (newPassword && newPassword.value) {
            if (newPassword.value !== confirmPassword.value) {
                showMessage('New passwords do not match', 'error');
                return;
            }
            if (newPassword.value.length < 6) {
                showMessage('Password must be at least 6 characters', 'error');
                return;
            }
        }

        if (saveBtn) {
            saveBtn.disabled = true;
            saveBtn.textContent = 'Saving...';
        }

        const formData = new URLSearchParams();
        const token = document.querySelector('input[name="__RequestVerificationToken"]');
        if (token) formData.append('__RequestVerificationToken', token.value);

        const fullNameField = document.getElementById('fullName');
        const emailField = document.getElementById('email');
        const currentPasswordField = document.getElementById('currentPassword');

        formData.append('fullName', fullNameField?.value || '');
        formData.append('email', emailField?.value || '');
        formData.append('currentPassword', currentPasswordField?.value || '');
        formData.append('newPassword', newPassword?.value || '');
        formData.append('confirmPassword', confirmPassword?.value || '');

        fetch('/Account/UpdateProfile', {
            method: 'POST',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
            body: formData
        })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    showMessage(data.message || 'Profile updated successfully!', 'success');
                    const fullNameInput = document.getElementById('fullName');
                    if (fullNameInput && fullNameInput.value) {
                        const nameElement = document.querySelector('.profile-header h2');
                        if (nameElement) nameElement.textContent = fullNameInput.value;
                        const initial = fullNameInput.value.trim().charAt(0) || 'U';
                        const avatarInitial = document.querySelector('.avatar-initial-large span');
                        if (avatarInitial) avatarInitial.textContent = initial.toUpperCase();
                    }
                    setTimeout(() => window.location.reload(), 2000);
                } else {
                    showMessage(data.message || 'Failed to update profile', 'error');
                    if (saveBtn) {
                        saveBtn.disabled = false;
                        saveBtn.textContent = 'Save Changes';
                    }
                }
            })
            .catch(error => {
                console.error('Error:', error);
                showMessage('An unexpected error occurred', 'error');
                if (saveBtn) {
                    saveBtn.disabled = false;
                    saveBtn.textContent = 'Save Changes';
                }
            });
    });
}

// ✅ FIXED: Anti‑forgery token sent as a form field, not as a header
function uploadPhotoToServer(file) {
    const formData = new FormData();
    formData.append('file', file);  

    // Add anti‑forgery token as a form field (required by [ValidateAntiForgeryToken])
    const token = document.querySelector('input[name="__RequestVerificationToken"]');
    if (token) formData.append('__RequestVerificationToken', token.value);

    const profileImg = document.getElementById('profilePhotoImg');
    if (profileImg) profileImg.style.opacity = '0.5';

    fetch('/Account/UpdateProfilePhoto', {
        method: 'POST',
        // No custom headers – let the browser set Content-Type automatically
        body: formData
    })
        .then(response => response.json())
        .then(data => {
            if (profileImg) profileImg.style.opacity = '1';
            if (data.success) {
                showMessage('Profile photo updated!', 'success');
                if (data.photoUrl && profileImg) {
                    profileImg.src = data.photoUrl;
                    const modalPreview = document.getElementById('modalPhotoPreview');
                    if (modalPreview) modalPreview.src = data.photoUrl;
                }
            } else {
                showMessage(data.message || 'Failed to update photo', 'error');
            }
        })
        .catch(error => {
            if (profileImg) profileImg.style.opacity = '1';
            console.error('Upload error:', error);
            showMessage('Error uploading photo', 'error');
        });
}

function showMessage(message, type) {
    let messageArea = document.getElementById('messageArea');
    if (!messageArea) {
        messageArea = document.createElement('div');
        messageArea.id = 'messageArea';
        messageArea.className = 'message-area';
        document.body.appendChild(messageArea);
    }
    messageArea.style.display = 'block';

    let bgColor, textColor, borderColor;
    if (type === 'success') {
        bgColor = 'rgba(34,197,94,0.2)';
        textColor = '#4ade80';
        borderColor = '#22c55e';
    } else {
        bgColor = 'rgba(239,68,68,0.2)';
        textColor = '#fca5a5';
        borderColor = '#ef4444';
    }

    const messageDiv = document.createElement('div');
    messageDiv.className = 'message';
    messageDiv.style.cssText = `padding:12px 16px;margin-bottom:10px;font-size:13px;animation:slideIn 0.3s ease;background:${bgColor};border-left:3px solid ${borderColor};color:${textColor};`;
    messageDiv.textContent = message;
    messageArea.appendChild(messageDiv);

    setTimeout(() => {
        if (messageDiv) messageDiv.remove();
        if (messageArea.children.length === 0) messageArea.style.display = 'none';
    }, 3000);
}
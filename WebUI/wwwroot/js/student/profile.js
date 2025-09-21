// Öðrenci profil sayfasý JavaScript fonksiyonlarý

document.addEventListener('DOMContentLoaded', function() {
    // Þifre doðrulama
    initializePasswordValidation();
    
    // Düzenleme modunu baþlat
    initializeEditMode();
    
    // Mesaj otomatik gizleme
    initializeAutoHideMessages();
});

// Þifre doðrulama iþlevselliði
function initializePasswordValidation() {
    const passwordForm = document.querySelector('.change-password-form');
    if (!passwordForm) return;
    
    const newPassword = passwordForm.querySelector('input[name="NewPassword"]');
    const confirmPassword = passwordForm.querySelector('input[name="ConfirmPassword"]');
    
    if (!newPassword || !confirmPassword) return;
    
    function validatePasswords() {
        confirmPassword.setCustomValidity(
            newPassword.value !== confirmPassword.value ? 'Þifreler eþleþmiyor' : ''
        );
    }
    
    newPassword.addEventListener('input', validatePasswords);
    confirmPassword.addEventListener('input', validatePasswords);
    
    passwordForm.addEventListener('submit', function(e) {
        if (newPassword.value !== confirmPassword.value) {
            e.preventDefault();
            alert('Þifreler eþleþmiyor. Lütfen kontrol edin.');
        }
    });
}

// Düzenleme modu iþlevselliði
function initializeEditMode() {
    const btn = document.getElementById('btnEditToggle');
    const display = document.getElementById('displaySection');
    const edit = document.getElementById('editSection');
    const cancelBtn = document.getElementById('btnCancelEdit');
    
    if (!btn || !display || !edit) return;
    
    function toEdit() {
        display.classList.add('hidden');
        edit.classList.remove('hidden');
        window.scrollTo({top: 0, behavior: 'smooth'});
    }
    
    function toDisplay() {
        edit.classList.add('hidden');
        display.classList.remove('hidden');
        window.scrollTo({top: 0, behavior: 'smooth'});
    }
    
    btn.addEventListener('click', toEdit);
    if (cancelBtn) {
        cancelBtn.addEventListener('click', toDisplay);
    }
}

// Mesajlarý otomatik gizleme
function initializeAutoHideMessages() {
    const messages = document.querySelectorAll('.bg-green-100, .bg-red-100');
    messages.forEach(function(message) {
        setTimeout(function() {
            message.style.transition = 'opacity 0.5s ease-out';
            message.style.opacity = '0';
            setTimeout(function() {
                if (message.parentNode) {
                    message.remove();
                }
            }, 500);
        }, 5000);
    });
}
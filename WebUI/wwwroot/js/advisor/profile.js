// Danýþman profil sayfasý JavaScript fonksiyonlarý

document.addEventListener('DOMContentLoaded', function() {
    initializeEditToggle();
});

function initializeEditToggle() {
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
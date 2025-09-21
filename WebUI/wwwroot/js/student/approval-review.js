// Onay formu gözden geçirilme sayfasý JavaScript fonksiyonlarý

document.addEventListener('DOMContentLoaded', function() {
    setupFormSubmission();
});

function setupFormSubmission() {
    const form = document.querySelector('form');
    const submitBtn = document.getElementById('submitBtn');
    
    if (form && submitBtn) {
        form.addEventListener('submit', function(e) {
            submitBtn.innerHTML = '<i class="fas fa-spinner fa-spin mr-2"></i>Gönderiliyor...';
            submitBtn.disabled = true;
        });
    }
}
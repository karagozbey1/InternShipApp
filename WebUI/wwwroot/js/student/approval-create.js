// Öðrenci onay formu oluþturma sayfasý JavaScript fonksiyonlarý

document.addEventListener('DOMContentLoaded', function() {
    initializeDateValidation();
});

function initializeDateValidation() {
    // Tarih alanlarý için minimum tarih ayarla
    const today = new Date().toISOString().split('T')[0];
    const startDateField = document.getElementById('StartDate');
    const endDateField = document.getElementById('EndDate');
    
    if (startDateField) {
        startDateField.setAttribute('min', today);
        
        startDateField.addEventListener('change', function() {
            const startDate = this.value;
            if (endDateField) {
                endDateField.setAttribute('min', startDate);
            }
        });
    }
}
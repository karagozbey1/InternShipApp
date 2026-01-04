// Modern Hesap Sayfalarý JavaScript - Basit ve Temiz

document.addEventListener('DOMContentLoaded', function() {
    // Body'ye modern layout sýnýfý ekle
    document.body.classList.add('modern-account-layout');
    
    // Body'ye yüzen þekiller ekle
    addFloatingShapes();
    
    // Form etkileþimlerini baþlat
    initFormInteractions();
    
    // Þifre göster/gizle özelliðini baþlat
    initPasswordToggle();
    
    // Üniversite dropdown'ýný baþlat
    initUniversityDropdown();
});

// Body'ye yüzen þekiller ekle
function addFloatingShapes() {
    if (document.querySelector('.floating-shapes')) return;

    const shapesContainer = document.createElement('div');
    shapesContainer.className = 'floating-shapes';
    
    for (let i = 1; i <= 4; i++) {
        const shape = document.createElement('div');
        shape.className = `shape shape-${i}`;
        shapesContainer.appendChild(shape);
    }
    
    document.body.appendChild(shapesContainer);

    // Þekillere fare etkileþimi ekle
    const shapes = document.querySelectorAll('.shape');
    shapes.forEach((shape, index) => {
        shape.addEventListener('mouseenter', function() {
            this.style.animationPlayState = 'paused';
            this.style.transform = 'scale(1.2)';
        });

        shape.addEventListener('mouseleave', function() {
            this.style.animationPlayState = 'running';
            this.style.transform = 'scale(1)';
        });
    });
}

// Form etkileþimlerini baþlat
function initFormInteractions() {
    // Tüm inputlar için metin rengini zorla
    const allInputs = document.querySelectorAll('input, textarea, select');
    allInputs.forEach(input => {
        // Baþlangýç rengini ayarla
        input.style.color = '#2d3748';
        input.style.webkitTextFillColor = '#2d3748';
        
        // Tüm olaylarda rengi koru
        ['focus', 'blur', 'input', 'change'].forEach(event => {
            input.addEventListener(event, function() {
                this.style.color = '#2d3748';
                this.style.webkitTextFillColor = '#2d3748';
            });
        });
    });

    // Geliþmiþ form doðrulama görüntüsü
    const forms = document.querySelectorAll('form');
    forms.forEach(form => {
        form.addEventListener('submit', function(e) {
            // Gönder butonuna yükleniyor durumu ekle
            const submitBtn = this.querySelector('button[type="submit"]');
            if (submitBtn) {
                submitBtn.innerHTML = '<span class="spinner-border spinner-border-sm" role="status"></span> Yükleniyor...';
                submitBtn.disabled = true;
            }
        });
    });
}

// Þifre göster/gizle iþlevselliði
function initPasswordToggle() {
    const toggleButtons = document.querySelectorAll('.toggle-password');
    toggleButtons.forEach(button => {
        button.addEventListener('click', function() {
            const targetSelector = this.getAttribute('toggle');
            const targetField = document.querySelector(targetSelector);
            
            if (targetField) {
                if (targetField.type === 'password') {
                    targetField.type = 'text';
                    this.classList.remove('fa-eye');
                    this.classList.add('fa-eye-slash');
                } else {
                    targetField.type = 'password';
                    this.classList.remove('fa-eye-slash');
                    this.classList.add('fa-eye');
                }
            }
        });
    });
}

// Üniversite dropdown iþlevselliði
function initUniversityDropdown() {
    const universityDisplay = document.getElementById('university-display');
    const universityList = document.getElementById('university-list');
    const selectedUniversityName = document.getElementById('selected-university-name');
    
    if (!universityDisplay || !universityList || !selectedUniversityName) return;

    const searchBox = universityList.querySelector('.search-box input');
    const universityItems = universityList.querySelectorAll('li');

    // Dropdown'ý aç/kapat
    universityDisplay.addEventListener('click', function() {
        universityList.classList.toggle('active');
    });

    // Üniversite seç
    universityItems.forEach(item => {
        item.addEventListener('click', function() {
            const universityName = this.getAttribute('data-name');
            universityDisplay.value = universityName;
            selectedUniversityName.value = universityName;
            universityList.classList.remove('active');
            
            // Doðrulamayý tetikle
            universityDisplay.dispatchEvent(new Event('change'));
        });
    });

    // Arama iþlevselliði
    if (searchBox) {
        searchBox.addEventListener('input', function() {
            const searchTerm = this.value.toLowerCase();
            universityItems.forEach(item => {
                const universityName = item.textContent.toLowerCase();
                if (universityName.includes(searchTerm)) {
                    item.style.display = 'block';
                } else {
                    item.style.display = 'none';
                }
            });
        });
    }

    // Dýþarý týklandýðýnda dropdown'ý kapat
    document.addEventListener('click', function(e) {
        if (!universityDisplay.contains(e.target) && !universityList.contains(e.target)) {
            universityList.classList.remove('active');
        }
    });
}

// 5 saniye sonra uyarýlarý otomatik gizle
setTimeout(() => {
    const alerts = document.querySelectorAll('.alert');
    alerts.forEach(alert => {
        if (alert) {
            alert.style.opacity = '0';
            alert.style.transform = 'translateY(-10px)';
            setTimeout(() => {
                alert.style.display = 'none';
            }, 300);
        }
    });
}, 5000);

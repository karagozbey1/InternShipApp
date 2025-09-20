document.addEventListener("DOMContentLoaded", function () {
    // Dashboard hazır olduğunda çalışacak fonksiyonlar
    
    // Progress bar animasyonlarını başlat
    initProgressAnimations();
    
    // İstatistik kartları hover efektlerini başlat
    initStatCardEffects();
    
    // Aktivite kartları etkileşimini başlat
    initActivityCardEffects();
    
    // Grafik güncelleme işlemlerini başlat
    initChartRefresh();
    
    // Otomatik yenileme işlemini başlat
    initAutoRefresh();
});

// Progress bar animasyonu - yumuşak doldurma efekti
function initProgressAnimations() {
    // Tüm progress bar elementlerini seç
    const progressBars = document.querySelectorAll('.progress-fill');
    
    // Her progress bar için animasyon uygula
    progressBars.forEach((bar, index) => {
        // Hedef genişlik değerini al
        const targetWidth = bar.style.width || '0%';
        // Başlangıçta genişliği sıfırla
        bar.style.width = '0%';
        
        // Gecikme ile animasyonu başlat
        setTimeout(() => {
            // Smooth geçiş efekti ekle
            bar.style.transition = 'width 1.5s cubic-bezier(0.4, 0, 0.2, 1)';
            // Hedef genişliğe geçiş yap
            bar.style.width = targetWidth;
        }, 300 + (index * 200)); // Her bar için farklı gecikme
    });
}

// İstatistik kartları hover efektleri
function initStatCardEffects() {
    // Tüm istatistik kartlarını seç
    const statCards = document.querySelectorAll('.stats .card');
    
    // Her kart için hover efektleri tanımla
    statCards.forEach(card => {
        // Mouse üzerine geldiğinde
        card.addEventListener('mouseenter', function() {
            // Yukarı kaydırma ve büyütme efekti
            this.style.transform = 'translateY(-8px) scale(1.02)'
            // Yumuşak geçiş animasyonu
            this.style.transition = 'all 0.3s cubic-bezier(0.4, 0, 0.2, 1)'
            // Gölge efekti artır
            this.style.boxShadow = '0 20px 40px rgba(0, 0, 0, 0.12)'
        });
        
        // Mouse çıktığında
        card.addEventListener('mouseleave', function() {
            // Normal pozisyona dön
            this.style.transform = 'translateY(0) scale(1)';
            // Normal gölge efektine dön
            this.style.boxShadow = '0 4px 6px rgba(0, 0, 0, 0.05)';
        });
    });
}

// Aktivite kartları etkileşimi
function initActivityCardEffects() {
    // Tüm aktivite satırlarını seç
    const activityRows = document.querySelectorAll('.activity-row');
    
    // Her aktivite satırı için etkileşim tanımla
    activityRows.forEach(row => {
        // Tıklama olayı
        row.addEventListener('click', function() {
            // Tıklama animasyonu - küçültme efekti
            this.style.transform = 'scale(0.98)';
            // Kısa süre sonra normal boyuta dön
            setTimeout(() => {
                this.style.transform = '';
            }, 150);
        });
        
        // Mouse üzerine gelme efekti
        row.addEventListener('mouseenter', function() {
            // Sağa kaydırma efekti
            this.style.transform = 'translateX(8px)';
            // Gölge efekti ekle
            this.style.boxShadow = '0 4px 12px rgba(0, 0, 0, 0.1)';
        });
        
        // Mouse çıkma efekti
        row.addEventListener('mouseleave', function() {
            // Normal pozisyona dön
            this.style.transform = 'translateX(0)';
            // Gölge efektini kaldır
            this.style.boxShadow = 'none';
        });
    });
}

// Grafik güncelleme başlatıcısı
function initChartRefresh() {
    // Dashboard verilerini düzenli aralıklarla güncelle
    // (Bu fonksiyon gelecekte genişletilebilir)
}

// Otomatik yenileme başlatıcısı
function initAutoRefresh() {
    // 5 dakikada bir dashboard verilerini güncelle
    setInterval(() => {
        // Dashboard verilerini sunucudan çek ve güncelle
        refreshDashboardData();
    }, 300000); // 5 dakika = 300000 ms
}

// Dashboard verilerini yenileme fonksiyonu
function refreshDashboardData() {
    // AJAX ile güncel verileri çek
    fetch('/Student/Dashboard/GetUpdatedData')
    .then(response => response.text())
    .then(html => {
        // Yeni HTML içeriğini parse et
        const parser = new DOMParser();
        const doc = parser.parseFromString(html, 'text/html');
        
        // İstatistik kartlarını güncelle
        updateStatCards(doc);
        
        // Aktiviteleri güncelle
        updateActivities(doc);
    })
    .catch(error => {
        // Hata durumunda sessizce devam et
        // Kullanıcıyı rahatsız etmemek için log gösterme
    });
}

// İstatistik kartlarını güncelleme fonksiyonu
function updateStatCards(newDoc) {
    // Mevcut istatistik değerlerini al
    const currentStats = document.querySelectorAll('.stats .big');
    // Yeni istatistik değerlerini al
    const newStats = newDoc.querySelectorAll('.stats .big');
    
    // Her istatistik için karşılaştır ve güncelle
    currentStats.forEach((stat, index) => {
        if (newStats[index] && stat.textContent !== newStats[index].textContent) {
            // Sayı değişimi animasyonu ile güncelle
            animateNumberChange(stat, newStats[index].textContent);
        }
    });
}

// Sayı değişim animasyon fonksiyonu
function animateNumberChange(element, newValue) {
    // Mevcut ve hedef değerleri al
    const currentValue = parseInt(element.textContent) || 0;
    const targetValue = parseInt(newValue) || 0;
    // Animasyon süre ve adım ayarları
    const duration = 800;
    const steps = 20;
    const increment = (targetValue - currentValue) / steps;
    
    let currentStep = 0;
    // Belirli aralıklarla sayıyı artır
    const interval = setInterval(() => {
        currentStep++;
        // Yeni değeri hesapla
        const value = Math.round(currentValue + (increment * currentStep));
        element.textContent = value;
        
        // Animasyon tamamlandığında durdur
        if (currentStep >= steps) {
            clearInterval(interval);
            element.textContent = newValue; // Final değeri ata
        }
    }, duration / steps);
}

// Aktiviteleri güncelleme fonksiyonu
function updateActivities(newDoc) {
    // Mevcut aktivite konteynerini al
    const currentActivities = document.querySelector('.activities-card .card-body');
    // Yeni aktivite konteynerini al
    const newActivities = newDoc.querySelector('.activities-card .card-body');
    
    // İçerik farklıysa güncelle
    if (currentActivities && newActivities && 
        currentActivities.innerHTML !== newActivities.innerHTML) {
        
        // Fade out efekti uygula
        currentActivities.style.opacity = '0.5';
        
        // Kısa gecikme sonrası içeriği güncelle
        setTimeout(() => {
            currentActivities.innerHTML = newActivities.innerHTML;
            // Fade in efekti uygula
            currentActivities.style.opacity = '1';
            
            // Yeni aktivite kartlarına event listener ekle
            initActivityCardEffects();
        }, 300);
    }
}

// Tema değişikliği için grafik renk güncelleme
document.addEventListener('themeChanged', function(e) {
    // Tema durumunu al
    const isDark = e.detail.isDark;
    
    // Grafik renkleri güncelle (eğer Chart.js kullanılıyorsa)
    if (window.Chart && window.internshipChart) {
        // Tema rengine göre metin rengi belirle
        const textColor = isDark ? '#e5e7eb' : '#374151';
        
        // Grafik etiket rengini güncelle
        window.internshipChart.options.plugins.legend.labels.color = textColor;
        // Grafiği yeniden çiz
        window.internshipChart.update();
    }
});

// Responsive grafik ayarları - ekran boyutu değiştiğinde
window.addEventListener('resize', function() {
    // Chart.js grafiği varsa yeniden boyutlandır
    if (window.Chart && window.internshipChart) {
        window.internshipChart.resize();
    }
});

// Yükleniyor durumu gösterici yardımcı fonksiyon
function showLoadingState(element) {
    // Orijinal içeriği sakla
    const originalContent = element.innerHTML;
    // Spinner ekle
    element.innerHTML = '<div class="spinner-border spinner-border-sm" role="status"><span class="sr-only">Yükleniyor...</span></div>';
    
    // Orijinal içeriği geri döndüren fonksiyon
    return () => {
        element.innerHTML = originalContent;
    };
}

// Global hata yakalama (sessiz mod)
window.addEventListener('error', function(e) {
    // Hatayı sessizce yakala, kullanıcıya gösterme
    // Geliştirme ortamında hata detayları kontrol edilebilir
});

// Dashboard için utility fonksiyonları buraya eklenebilir

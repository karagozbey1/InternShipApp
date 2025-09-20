/**
 * Öğrenci Dashboard JavaScript Modülü
 * 
 * Bu dosya öğrenci dashboard sayfasının tüm etkileşimli özelliklerini yönetir
 * İçerdikler:
 * - Progress bar animasyonları
 * - İstatistik kartlarının hover efektleri  
 * - Aktivite listesi etkileşimleri
 * - Otomatik veri yenileme işlemleri
 * - Grafik güncelleme ve tema değişiklikleri
 * 
 * @author InternshipTrackingSystem
 * @version 1.0
 */

/**
 * DOM tamamen yüklendiğinde çalışacak ana başlatıcı fonksiyon
 * Tüm dashboard bileşenlerini initialize eder
 */
document.addEventListener("DOMContentLoaded", function () {
    console.log("🚀 Öğrenci Dashboard başlatılıyor...");
    
    // Progress bar animasyonlarını başlat
    initProgressAnimations();
    
    // İstatistik kartları hover efektlerini başlat
    initStatCardEffects();
    
    // Aktivite kartları etkileşimini başlat
    initActivityCardEffects();
    
    // Grafik güncelleme işlemlerini başlat
    initChartRefresh();
    
    // Otomatik yenileme işlemini başlat (5 dakikada bir)
    initAutoRefresh();
    
    console.log("✅ Dashboard bileşenleri başarıyla yüklendi");
});

/**
 * Progress bar animasyon sistemini başlatır
 * Tüm .progress-fill elementlerini bulup yumuşak dolum animasyonu uygular
 * Her progress bar farklı gecikme ile animasyona başlar (cascade efekt)
 */
function initProgressAnimations() {
    console.log("📊 Progress bar animasyonları başlatılıyor...");
    
    // Tüm progress bar elementlerini seç (.progress-fill class'ına sahip olanlar)
    const progressBars = document.querySelectorAll('.progress-fill');
    
    if (progressBars.length === 0) {
        console.warn("⚠️ Hiç progress bar bulunamadı");
        return;
    }
    
    // Her progress bar için sırayla animasyon uygula
    progressBars.forEach((bar, index) => {
        // Data attribute'den hedef yüzdeyi al, yoksa style'dan oku
        const targetWidth = bar.getAttribute('data-progress') || bar.style.width || '0%';
        
        // Başlangıçta genişliği sıfırla (animasyon için)
        bar.style.width = '0%';
        
        // Her bar için farklı gecikme ile animasyonu başlat
        // İlk bar 300ms, ikinci bar 500ms, üçüncü bar 700ms gecikme ile başlar
        setTimeout(() => {
            // CSS transition özelliğini ekle - smooth cubic-bezier animasyon
            bar.style.transition = 'width 1.5s cubic-bezier(0.4, 0, 0.2, 1)';
            
            // Hedef genişliğe geçiş yap
            bar.style.width = targetWidth;
            
            // Progress değerini console'da göster (debug için)
            console.log(`📈 Progress ${index + 1}: ${targetWidth}`);
        }, 300 + (index * 200)); // Cascade efekt için artan gecikme
    });
    
    console.log(`✅ ${progressBars.length} progress bar animasyonu ayarlandı`);
}

/**
 * İstatistik kartlarının hover efekt sistemini başlatır
 * Mouse over/out eventleri ile yukarı kaldırma ve gölge efektleri
 * Transform ve box-shadow CSS properties kullanır
 */
function initStatCardEffects() {
    console.log("🎯 İstatistik kartları efektleri başlatılıyor...");
    
    // Tüm istatistik kartlarını seç (.stats .card selector'u ile)
    const statCards = document.querySelectorAll('.stats .card');
    
    if (statCards.length === 0) {
        console.warn("⚠️ Hiç istatistik kartı bulunamadı");
        return;
    }
    
    // Her kart için hover event listener'ları tanımla
    statCards.forEach((card, index) => {
        // Mouse kartın üzerine geldiğinde çalışacak fonksiyon
        card.addEventListener('mouseenter', function() {
            // Yukarı kaydırma (-8px) ve hafif büyütme (%2) efekti
            this.style.transform = 'translateY(-8px) scale(1.02)';
            
            // Yumuşak geçiş animasyonu (0.3s cubic-bezier)
            this.style.transition = 'all 0.3s cubic-bezier(0.4, 0, 0.2, 1)';
            
            // Gölge efektini artır (daha belirgin ve yayılmış gölge)
            this.style.boxShadow = '0 20px 40px rgba(0, 0, 0, 0.12)';
            
            // Z-index'i artır ki diğer kartların üstünde dursun
            this.style.zIndex = '10';
        });
        
        // Mouse karttan çıktığında çalışacak fonksiyon
        card.addEventListener('mouseleave', function() {
            // Normal pozisyona dön (transform resetle)
            this.style.transform = 'translateY(0) scale(1)';
            
            // Normal gölge efektine dön (hafif gölge)
            this.style.boxShadow = '0 4px 6px rgba(0, 0, 0, 0.05)';
            
            // Z-index'i normale çevir
            this.style.zIndex = '';
        });
        
        // Tıklama efekti için click event (feedback amaçlı)
        card.addEventListener('click', function() {
            // Anlık küçültme efekti (kullanıcı feedback'i)
            this.style.transform = 'translateY(-8px) scale(0.98)';
            
            // 100ms sonra normal hover durumuna dön
            setTimeout(() => {
                this.style.transform = 'translateY(-8px) scale(1.02)';
            }, 100);
        });
    });
    
    console.log(`✅ ${statCards.length} istatistik kartı efekti ayarlandı`);
}

/**
 * Aktivite kartları etkileşim sistemini başlatır
 * Son aktivitelerin bulunduğu listede hover ve click efektleri
 * Sağa kaydırma hover efekti ve tıklama feedback'i
 */
function initActivityCardEffects() {
    console.log("📋 Aktivite kartları etkileşimi başlatılıyor...");
    
    // Tüm aktivite satırlarını seç (.activity-row class'ına sahip)
    const activityRows = document.querySelectorAll('.activity-row');
    
    if (activityRows.length === 0) {
        console.warn("⚠️ Hiç aktivite satırı bulunamadı");
        return;
    }
    
    // Her aktivite satırı için etkileşim tanımla
    activityRows.forEach((row, index) => {
        // Tıklama olayı - kullanıcı feedback'i için
        row.addEventListener('click', function() {
            console.log(`🖱️ Aktivite ${index + 1} tıklandı`);
            
            // Tıklama animasyonu - küçültme efekti (scale 0.98)
            this.style.transform = 'scale(0.98)';
            this.style.transition = 'transform 0.1s ease';
            
            // 150ms sonra normal boyuta dön
            setTimeout(() => {
                this.style.transform = '';
            }, 150);
            
            // Burada aktivite detayı açma işlemi yapılabilir
            // showActivityDetail(index) gibi...
        });
        
        // Mouse hover efektleri
        row.addEventListener('mouseenter', function() {
            // Sağa kaydırma efekti (8px translateX)
            this.style.transform = 'translateX(8px)';
            this.style.transition = 'all 0.2s ease';
            
            // Hafif gölge efekti ekle
            this.style.boxShadow = '0 4px 12px rgba(0, 0, 0, 0.1)';
            
            // Background rengini hafif değiştir (hover state)
            this.style.backgroundColor = 'rgba(59, 130, 246, 0.05)';
        });
        
        // Mouse leave efekti
        row.addEventListener('mouseleave', function() {
            // Normal pozisyona dön
            this.style.transform = 'translateX(0)';
            
            // Gölge efektini kaldır
            this.style.boxShadow = 'none';
            
            // Background rengini normale çevir
            this.style.backgroundColor = '';
        });
    });
    
    console.log(`✅ ${activityRows.length} aktivite satırı etkileşimi ayarlandı`);
}

/**
 * Grafik güncelleme sistemini başlatır
 * Chart.js veya benzeri grafik kütüphaneleri için hazırlık
 * Şu anda placeholder - gelecekte genişletilebilir
 */
function initChartRefresh() {
    console.log("📈 Grafik güncelleme sistemi hazır");
    
    // Chart.js veya diğer grafik kütüphaneleri için
    // Bu fonksiyon gelecekte genişletilebilir
    
    // Örnek: Grafik verilerini güncelleme fonksiyonu
    if (window.Chart && window.internshipChart) {
        console.log("📊 Chart.js grafiği tespit edildi");
        
        // Grafik update işlemi burada yapılabilir
        // window.internshipChart.update();
    }
}

/**
 * Otomatik veri yenileme sistemini başlatır
 * 5 dakikada bir dashboard verilerini AJAX ile günceller
 * Kullanıcı deneyimini bozmadan arka planda çalışır
 */
function initAutoRefresh() {
    console.log("🔄 Otomatik yenileme sistemi başlatılıyor...");
    
    // İlk yenileme 5 dakika sonra başlar
    const refreshInterval = 5 * 60 * 1000; // 5 dakika = 300000 ms
    
    // Setinterval ile düzenli aralıklarla yenileme
    const intervalId = setInterval(() => {
        console.log("🔄 Dashboard verileri yenileniyor...");
        
        // Ana veri yenileme fonksiyonunu çağır
        refreshDashboardData();
    }, refreshInterval);
    
    // Sayfa kapatıldığında interval'i temizle (memory leak önlemi)
    window.addEventListener('beforeunload', () => {
        clearInterval(intervalId);
        console.log("🧹 Auto-refresh interval temizlendi");
    });
    
    console.log(`✅ Otomatik yenileme ${refreshInterval / 1000}s aralıklarla ayarlandı`);
}

/**
 * Dashboard verilerini AJAX ile yeniler
 * Sunucudan güncel HTML alıp mevcut içeriği günceller
 * Animasyonlu geçiş efektleri ile kullanıcı deneyimi sağlar
 */
function refreshDashboardData() {
    console.log("🌐 Sunucudan güncel veriler çekiliyor...");
    
    // AJAX ile dashboard verilerini çek
    fetch('/Student/Dashboard/GetUpdatedData', {
        method: 'GET',
        headers: {
            'Content-Type': 'application/json',
            // CSRF token varsa eklenebilir
            'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value || ''
        }
    })
    .then(response => {
        if (!response.ok) {
            throw new Error(`HTTP Error: ${response.status}`);
        }
        return response.text();
    })
    .then(html => {
        console.log("✅ Güncel veriler alındı");
        
        // Yeni HTML içeriğini parse et
        const parser = new DOMParser();
        const doc = parser.parseFromString(html, 'text/html');
        
        // İstatistik kartlarını güncelle
        updateStatCards(doc);
        
        // Aktivite listesini güncelle
        updateActivities(doc);
        
        console.log("🔄 Dashboard güncelleme tamamlandı");
    })
    .catch(error => {
        // Hata durumunda sessizce devam et
        // Kullanıcıyı rahatsız etmemek için console'da log tut
        console.warn("⚠️ Dashboard yenileme hatası:", error.message);
        
        // Production'da hata raporlama sistemi ile gönderilebilir
        // reportError('dashboard_refresh_failed', error);
    });
}

/**
 * İstatistik kartlarının değerlerini günceller
 * Sayı değişikliklerini animasyonlu şekilde gösterir
 * 
 * @param {Document} newDoc - Sunucudan alınan yeni HTML document
 */
function updateStatCards(newDoc) {
    console.log("📊 İstatistik kartları güncelleniyor...");
    
    // Mevcut istatistik değerlerini al (.stats .big selector ile)
    const currentStats = document.querySelectorAll('.stats .big');
    // Yeni istatistik değerlerini al
    const newStats = newDoc.querySelectorAll('.stats .big');
    
    // Her istatistik için karşılaştır ve güncelle
    currentStats.forEach((stat, index) => {
        if (newStats[index] && stat.textContent !== newStats[index].textContent) {
            console.log(`📈 İstatistik ${index + 1} güncelleniyor: ${stat.textContent} → ${newStats[index].textContent}`);
            
            // Sayı değişimi animasyonu ile güncelle
            animateNumberChange(stat, newStats[index].textContent);
        }
    });
}

/**
 * Sayı değişimlerini animasyonlu şekilde gösterir
 * Mevcut değerden yeni değere yumuşak geçiş yapar
 * 
 * @param {HTMLElement} element - Güncellenecek HTML elementi
 * @param {string} newValue - Yeni gösterilecek değer
 */
function animateNumberChange(element, newValue) {
    // Mevcut ve hedef değerleri integer'a çevir
    const currentValue = parseInt(element.textContent.replace(/[^\d]/g, '')) || 0;
    const targetValue = parseInt(newValue.replace(/[^\d]/g, '')) || 0;
    
    // Animasyon parametreleri
    const duration = 800; // 800ms toplam süre
    const steps = 20;     // 20 adımda tamamla
    const increment = (targetValue - currentValue) / steps; // Her adımdaki artış miktarı
    
    let currentStep = 0;
    
    // Belirli aralıklarla sayıyı artır (düzgün animasyon için)
    const interval = setInterval(() => {
        currentStep++;
        
        // Yeni değeri hesapla (yuvarla)
        const value = Math.round(currentValue + (increment * currentStep));
        
        // Elementte göster (orijinal format korunarak)
        element.textContent = newValue.replace(/\d+/, value.toString());
        
        // Animasyon tamamlandığında durdur
        if (currentStep >= steps) {
            clearInterval(interval);
            element.textContent = newValue; // Final değeri kesin olarak ata
            
            console.log(`✅ Sayı animasyonu tamamlandı: ${newValue}`);
        }
    }, duration / steps);
    
    // Element'e animasyon class'ı ekle (CSS ile ek efekt)
    element.classList.add('number-updating');
    
    // Animasyon bitince class'ı kaldır
    setTimeout(() => {
        element.classList.remove('number-updating');
    }, duration);
}

/**
 * Aktivite listesini günceller
 * Yeni aktiviteler varsa fade efekti ile değiştirir
 * 
 * @param {Document} newDoc - Sunucudan alınan yeni HTML document
 */
function updateActivities(newDoc) {
    console.log("📋 Aktivite listesi güncelleniyor...");
    
    // Mevcut aktivite konteynerini al
    const currentActivities = document.querySelector('.activities-card .card-body');
    // Yeni aktivite konteynerini al
    const newActivities = newDoc.querySelector('.activities-card .card-body');
    
    // İçerik kontrolü - farklıysa güncelle
    if (currentActivities && newActivities && 
        currentActivities.innerHTML !== newActivities.innerHTML) {
        
        console.log("🔄 Aktivite içeriği değişti, güncelleniyor...");
        
        // Fade out efekti uygula (şeffaflaştır)
        currentActivities.style.transition = 'opacity 0.3s ease';
        currentActivities.style.opacity = '0.5';
        
        // Kısa gecikme sonrası içeriği güncelle
        setTimeout(() => {
            // İçeriği değiştir
            currentActivities.innerHTML = newActivities.innerHTML;
            
            // Fade in efekti uygula (görünür yap)
            currentActivities.style.opacity = '1';
            
            // Yeni aktivite kartlarına event listener ekle
            initActivityCardEffects();
            
            console.log("✅ Aktivite listesi güncellendi");
        }, 300);
    } else {
        console.log("ℹ️ Aktivite içeriği değişmemiş");
    }
}

/**
 * Tema değişikliği event handler'ı
 * Dark/Light mode geçişlerinde grafik renklerini günceller
 */
document.addEventListener('themeChanged', function(e) {
    console.log(`🎨 Tema değişti: ${e.detail.isDark ? 'Dark' : 'Light'} mode`);
    
    // Tema durumunu al
    const isDark = e.detail.isDark;
    
    // Chart.js grafiği varsa renklerini güncelle
    if (window.Chart && window.internshipChart) {
        console.log("📊 Grafik renkleri güncelleniyor...");
        
        // Tema rengine göre metin rengi belirle
        const textColor = isDark ? '#e5e7eb' : '#374151';
        const gridColor = isDark ? '#374151' : '#e5e7eb';
        
        // Grafik legend ve axis renklerini güncelle
        if (window.internshipChart.options.plugins?.legend?.labels) {
            window.internshipChart.options.plugins.legend.labels.color = textColor;
        }
        
        // Grid renkleri güncelle (varsa)
        if (window.internshipChart.options.scales) {
            Object.keys(window.internshipChart.options.scales).forEach(scaleKey => {
                const scale = window.internshipChart.options.scales[scaleKey];
                if (scale.ticks) scale.ticks.color = textColor;
                if (scale.grid) scale.grid.color = gridColor;
            });
        }
        
        // Grafiği yeniden çiz
        window.internshipChart.update('none'); // Animasyonsuz güncelleme
        
        console.log("✅ Grafik tema renkleri güncellendi");
    }
});

/**
 * Responsive design için window resize event handler'ı
 * Ekran boyutu değiştiğinde grafikleri yeniden boyutlandırır
 */
window.addEventListener('resize', function() {
    console.log("📱 Ekran boyutu değişti, grafikler yeniden boyutlandırılıyor...");
    
    // Chart.js grafiği varsa yeniden boyutlandır
    if (window.Chart && window.internshipChart) {
        window.internshipChart.resize();
        console.log("📊 Grafik boyutlandırıldı");
    }
    
    // Progress barları yeniden hesapla (responsive için)
    const progressBars = document.querySelectorAll('.progress-fill');
    progressBars.forEach(bar => {
        // Force reflow için width'i yeniden set et
        const currentWidth = bar.style.width;
        bar.style.width = currentWidth;
    });
});

/**
 * Yükleniyor durumu gösterici yardımcı fonksiyon
 * Button veya element'lere loading state ekler
 * 
 * @param {HTMLElement} element - Loading state eklenecek element
 * @returns {Function} - Orijinal içeriği geri döndüren fonksiyon
 */
function showLoadingState(element) {
    // Orijinal içeriği sakla
    const originalContent = element.innerHTML;
    
    // Spinner HTML'i oluştur
    const spinnerHtml = `
        <div class="d-flex align-items-center">
            <div class="spinner-border spinner-border-sm me-2" role="status">
                <span class="visually-hidden">Yükleniyor...</span>
            </div>
            Yükleniyor...
        </div>
    `;
    
    // Element'e spinner ekle
    element.innerHTML = spinnerHtml;
    element.disabled = true; // Button ise disable et
    
    console.log("⏳ Loading state aktif");
    
    // Orijinal içeriği geri döndüren fonksiyon
    return () => {
        element.innerHTML = originalContent;
        element.disabled = false;
        console.log("✅ Loading state kaldırıldı");
    };
}

/**
 * Global hata yakalama sistemi
 * JavaScript hatalarını sessizce yakalar ve loglar
 * Production'da hata raporlama servisi ile entegre edilebilir
 */
window.addEventListener('error', function(e) {
    // Hatayı sessizce yakala, kullanıcıya gösterme
    console.warn("🚨 JavaScript Hatası:", {
        message: e.message,
        filename: e.filename,
        lineno: e.lineno,
        colno: e.colno,
        error: e.error
    });
    
    // Production'da hata raporlama sistemi
    // if (window.errorReporting) {
    //     window.errorReporting.captureException(e.error);
    // }
});

/**
 * Promise rejection hataları için global handler
 * Async/await hatalarını yakalar
 */
window.addEventListener('unhandledrejection', function(e) {
    console.warn("🚨 Promise Rejection:", e.reason);
    
    // Promise hatasını görmezden gel (prevent console error)
    e.preventDefault();
});

/**
 * Dashboard utility fonksiyonları namespace'i
 * Diğer dosyalar tarafından kullanılabilecek yardımcı fonksiyonlar
 */
window.DashboardUtils = {
    /**
     * Bildirim gösterici fonksiyonu
     * Toast notification ekler
     */
    showNotification: function(message, type = 'info', duration = 3000) {
        console.log(`🔔 Bildirim: ${message} (${type})`);
        
        // Toast container'ı oluştur (yoksa)
        let toastContainer = document.querySelector('.toast-container');
        if (!toastContainer) {
            toastContainer = document.createElement('div');
            toastContainer.className = 'toast-container position-fixed top-0 end-0 p-3';
            toastContainer.style.zIndex = '9999';
            document.body.appendChild(toastContainer);
        }
        
        // Toast HTML'i oluştur
        const toastId = `toast-${Date.now()}`;
        const toastHtml = `
            <div id="${toastId}" class="toast align-items-center text-white bg-${type === 'error' ? 'danger' : type === 'success' ? 'success' : 'primary'} border-0" role="alert">
                <div class="d-flex">
                    <div class="toast-body">${message}</div>
                    <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
                </div>
            </div>
        `;
        
        // Toast'u container'a ekle
        toastContainer.insertAdjacentHTML('beforeend', toastHtml);
        
        // Bootstrap toast'unu initialize et ve göster
        const toastElement = document.getElementById(toastId);
        const toast = new bootstrap.Toast(toastElement, { delay: duration });
        toast.show();
        
        // Toast kapandığında DOM'dan kaldır
        toastElement.addEventListener('hidden.bs.toast', () => {
            toastElement.remove();
        });
    },
    
    /**
     * CSRF token'ı alır
     */
    getCSRFToken: function() {
        return document.querySelector('input[name="__RequestVerificationToken"]')?.value || '';
    },
    
    /**
     * API çağrısı için standart fetch wrapper'ı
     */
    apiCall: async function(url, options = {}) {
        const defaultOptions = {
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': this.getCSRFToken()
            }
        };
        
        const response = await fetch(url, { ...defaultOptions, ...options });
        
        if (!response.ok) {
            throw new Error(`API Error: ${response.status} ${response.statusText}`);
        }
        
        return response;
    }
};

console.log("📋 Dashboard JavaScript modülü yüklendi - Tüm fonksiyonlar hazır");

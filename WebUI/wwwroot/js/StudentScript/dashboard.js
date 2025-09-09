document.addEventListener("DOMContentLoaded", function () {
    console.log("Student dashboard hazır");
    
    // Progress bar animasyonu
    initProgressAnimations();
    
    // Stat kartları hover efektleri
    initStatCardEffects();
    
    // Aktivite kartları etkileşimi
    initActivityCardEffects();
    
    // Chart güncelleme
    initChartRefresh();
    
    // Auto refresh
    initAutoRefresh();
});

// Progress bar animasyonu - resimdeki gibi smooth doldurma
function initProgressAnimations() {
    const progressBars = document.querySelectorAll('.progress-fill');
    
    progressBars.forEach((bar, index) => {
        const targetWidth = bar.style.width || '0%';
        bar.style.width = '0%';
        
        setTimeout(() => {
            bar.style.transition = 'width 1.5s cubic-bezier(0.4, 0, 0.2, 1)';
            bar.style.width = targetWidth;
        }, 300 + (index * 200));
    });
}

// Stat kartları hover efektleri
function initStatCardEffects() {
    const statCards = document.querySelectorAll('.stats .card');
    
    statCards.forEach(card => {
        card.addEventListener('mouseenter', function() {
            this.style.transform = 'translateY(-8px) scale(1.02)'
            this.style.transition = 'all 0.3s cubic-bezier(0.4, 0, 0.2, 1)'
            this.style.boxShadow = '0 20px 40px rgba(0, 0, 0, 0.12)'
        });
        
        card.addEventListener('mouseleave', function() {
            this.style.transform = 'translateY(0) scale(1)';
            this.style.boxShadow = '0 4px 6px rgba(0, 0, 0, 0.05)';
        });
    });
}

// Aktivite kartları etkileşimi
function initActivityCardEffects() {
    const activityRows = document.querySelectorAll('.activity-row');
    
    activityRows.forEach(row => {
        row.addEventListener('click', function() {
            // Tıklama animasyonu
            this.style.transform = 'scale(0.98)';
            setTimeout(() => {
                this.style.transform = '';
            }, 150);
        });
        
        // Hover efekti
        row.addEventListener('mouseenter', function() {
            this.style.transform = 'translateX(8px)';
            this.style.boxShadow = '0 4px 12px rgba(0, 0, 0, 0.1)';
        });
        
        row.addEventListener('mouseleave', function() {
            this.style.transform = 'translateX(0)';
            this.style.boxShadow = 'none';
        });
    });
}

// Chart refresh fonksiyonu
function initChartRefresh() {
    const chartContainer = document.getElementById('internshipChart');
    if (chartContainer) {
        // Chart.js instance varsa, responsive ayarlar
        if (window.Chart) {
            Chart.defaults.font.family = "'Inter', system-ui, -apple-system, sans-serif";
            Chart.defaults.color = getComputedStyle(document.documentElement).getPropertyValue('--text').trim();
        }
    }
}

// Otomatik güncelleme (5 dakikada bir)
function initAutoRefresh() {
    // Sadece development modunda debug için
    if (window.location.hostname === 'localhost') {
        console.log('Auto refresh aktif - her 5 dakikada yenilenir');
        
        setInterval(() => {
            // Saddle sayfayı yenile değil, AJAX ile veri güncelle
            refreshDashboardData();
        }, 5 * 60 * 1000); // 5 dakika
    }
}

// Dashboard verilerini AJAX ile güncelle
function refreshDashboardData() {
    fetch(window.location.href, {
        method: 'GET',
        headers: {
            'X-Requested-With': 'XMLHttpRequest'
        }
    })
    .then(response => response.text())
    .then(html => {
        // Sadece değişen kısımları güncelle
        const parser = new DOMParser();
        const doc = parser.parseFromString(html, 'text/html');
        
        // İstatistik kartlarını güncelle
        updateStatCards(doc);
        
        // Aktiviteleri güncelle
        updateActivities(doc);
        
        console.log('Dashboard verileri güncellendi');
    })
    .catch(error => {
        console.error('Dashboard güncelleme hatası:', error);
    });
}

// Stat kartlarını güncelle
function updateStatCards(newDoc) {
    const currentStats = document.querySelectorAll('.stats .big');
    const newStats = newDoc.querySelectorAll('.stats .big');
    
    currentStats.forEach((stat, index) => {
        if (newStats[index] && stat.textContent !== newStats[index].textContent) {
            // Sayı değişim animasyonu
            animateNumberChange(stat, newStats[index].textContent);
        }
    });
}

// Sayı değişim animasyonu
function animateNumberChange(element, newValue) {
    const currentValue = parseInt(element.textContent) || 0;
    const targetValue = parseInt(newValue) || 0;
    const duration = 800;
    const steps = 20;
    const increment = (targetValue - currentValue) / steps;
    
    let currentStep = 0;
    const interval = setInterval(() => {
        currentStep++;
        const value = Math.round(currentValue + (increment * currentStep));
        element.textContent = value;
        
        if (currentStep >= steps) {
            clearInterval(interval);
            element.textContent = newValue;
        }
    }, duration / steps);
}

// Aktiviteleri güncelle
function updateActivities(newDoc) {
    const currentActivities = document.querySelector('.activities-card .card-body');
    const newActivities = newDoc.querySelector('.activities-card .card-body');
    
    if (currentActivities && newActivities && 
        currentActivities.innerHTML !== newActivities.innerHTML) {
        
        // Fade out -> update -> fade in
        currentActivities.style.opacity = '0.5';
        
        setTimeout(() => {
            currentActivities.innerHTML = newActivities.innerHTML;
            currentActivities.style.opacity = '1';
            
            // Yeni aktivite kartlarına event listener ekle
            initActivityCardEffects();
        }, 300);
    }
}

// Tema değişikliği için chart renk güncelleme
document.addEventListener('themeChanged', function(e) {
    const isDark = e.detail.isDark;
    
    // Chart renklerini güncelle
    if (window.Chart && window.internshipChart) {
        const textColor = isDark ? '#e5e7eb' : '#374151';
        
        window.internshipChart.options.plugins.legend.labels.color = textColor;
        window.internshipChart.update();
    }
});

// Responsive chart ayarları
window.addEventListener('resize', function() {
    if (window.Chart && window.internshipChart) {
        window.internshipChart.resize();
    }
});

// Loading state helper
function showLoadingState(element) {
    const originalContent = element.innerHTML;
    element.innerHTML = '<div class="spinner-border spinner-border-sm" role="status"><span class="sr-only">Yükleniyor...</span></div>';
    
    return () => {
        element.innerHTML = originalContent;
    };
}

// Error handling
window.addEventListener('error', function(e) {
    console.error('Dashboard error:', e.error);
});

// Dashboard için utility fonksiyonlar
const DashboardUtils = {
    // Değer formatla
    formatValue: function(value, type = 'number') {
        if (type === 'percentage') {
            return `${Math.round(value)}%`;
        }
        if (type === 'currency') {
            return new Intl.NumberFormat('tr-TR', { 
                style: 'currency', 
                currency: 'TRY' 
            }).format(value);
        }
        return value.toString();
    },
    
    // Tarih formatla
    formatDate: function(date) {
        return new Intl.DateTimeFormat('tr-TR', {
            day: 'numeric',
            month: 'long',
            year: 'numeric'
        }).format(new Date(date));
    },
    
    // Renk tonunu ayarla
    adjustColor: function(color, amount) {
        const usePound = color[0] === '#';
        const col = usePound ? color.slice(1) : color;
        const num = parseInt(col, 16);
        let r = (num >> 16) + amount;
        let g = (num >> 8 & 0x00FF) + amount;
        let b = (num & 0x0000FF) + amount;
        
        r = r > 255 ? 255 : r < 0 ? 0 : r;
        g = g > 255 ? 255 : g < 0 ? 0 : g;
        b = b > 255 ? 255 : b < 0 ? 0 : b;
        
        return (usePound ? '#' : '') + (r << 16 | g << 8 | b).toString(16);
    }
};

// Global export
window.DashboardUtils = DashboardUtils;

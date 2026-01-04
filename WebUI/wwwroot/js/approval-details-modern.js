/**
 * Modern Onay Detaylarý JavaScript
 * Onay formu detay sayfasý için etkileþimleri ve animasyonlarý yönetir
 */

class ApprovalDetails {
    constructor() {
        this.init();
    }

    init() {
        this.bindEvents();
        this.initAnimations();
        this.setupStatusUpdates();
        this.handleNotifications();
    }

    bindEvents() {
        // Düzenle butonu týklama iþlemi
        document.addEventListener('click', (e) => {
            if (e.target.closest('.edit-btn')) {
                this.handleEditClick(e.target.closest('.edit-btn'));
            }
        });

        // Durum kartý hover efektleri
        const statusDisplays = document.querySelectorAll('.status-display');
        statusDisplays.forEach(display => {
            display.addEventListener('mouseenter', () => this.animateStatusCard(display, true));
            display.addEventListener('mouseleave', () => this.animateStatusCard(display, false));
        });

        // Doküman baðlantýsý týklamalarý
        document.addEventListener('click', (e) => {
            if (e.target.closest('.doc-view')) {
                this.handleDocumentView(e);
            }
        });
    }

    handleEditClick(button) {
        // Yükleniyor durumu ekle
        this.setButtonLoading(button, true);
        
        // Yumuþak geçiþ efekti ekle
        setTimeout(() => {
            this.setButtonLoading(button, false);
        }, 800);
    }

    setButtonLoading(button, loading) {
        if (loading) {
            button.classList.add('loading');
            const originalText = button.innerHTML;
            button.dataset.originalText = originalText;
            button.innerHTML = '<i class="fas fa-spinner fa-spin"></i><span>Yükleniyor...</span>';
        } else {
            button.classList.remove('loading');
            if (button.dataset.originalText) {
                button.innerHTML = button.dataset.originalText;
            }
        }
    }

    animateStatusCard(card, isHover) {
        if (isHover) {
            card.style.transform = 'translateY(-2px) scale(1.02)';
            card.style.boxShadow = '0 8px 25px rgba(0, 0, 0, 0.15)';
        } else {
            card.style.transform = 'translateY(0) scale(1)';
            card.style.boxShadow = '0 2px 8px rgba(0, 0, 0, 0.1)';
        }
    }

    initAnimations() {
        // Fade-in animasyonlarý için Intersection Observer
        const observerOptions = {
            threshold: 0.1,
            rootMargin: '0px 0px -50px 0px'
        };

        const observer = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    entry.target.classList.add('fade-in-active');
                }
            });
        }, observerOptions);

        // Tüm kartlarý gözlemle
        document.querySelectorAll('.fade-in').forEach(card => {
            observer.observe(card);
        });

        // Durum nabýz animasyonu
        const statusText = document.querySelector('.hero-status-text');
        if (statusText) {
            this.startStatusPulse(statusText);
        }
    }

    startStatusPulse(element) {
        setInterval(() => {
            element.style.animation = 'none';
            setTimeout(() => {
                element.style.animation = 'statusPulse 2s ease-in-out infinite';
            }, 10);
        }, 10000); // Her 10 saniyede nabýz at
    }

    setupStatusUpdates() {
        // Gerçek zamanlý durum güncellemeleri (gerekirse)
        const statusCard = document.querySelector('.status-display');
        if (statusCard) {
            // Gerçek zamanlý güncellemeleri simüle et (gerçek WebSocket/SignalR ile deðiþtirin)
            this.checkStatusUpdates();
        }
    }

    checkStatusUpdates() {
        // Bu, gerçek gerçek zamanlý uygulama ile deðiþtirilecek
        // Þimdilik gelecekteki geliþtirme için sadece bir yer tutucu
        console.log('Durum güncelleme kontrolü baþlatýldý');
    }

    handleDocumentView(e) {
        e.preventDefault();
        const link = e.target.closest('.doc-view');
        const href = link.getAttribute('href');
        
        // Yükleniyor efekti ekle
        link.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Açýlýyor...';
        
        // Dokümaný yeni sekmede aç
        setTimeout(() => {
            window.open(href, '_blank');
            link.innerHTML = 'Gör';
        }, 500);
    }

    handleNotifications() {
        // Baþarý/hata mesajlarýný kontrol et
        const urlParams = new URLSearchParams(window.location.search);
        const success = urlParams.get('success');
        const error = urlParams.get('error');

        if (success) {
            this.showNotification(decodeURIComponent(success), 'success');
        }
        if (error) {
            this.showNotification(decodeURIComponent(error), 'error');
        }
    }

    showNotification(message, type = 'info') {
        // Bildirim elementi oluþtur
        const notification = document.createElement('div');
        notification.className = `notification notification-${type}`;
        notification.innerHTML = `
            <div class="notification-content">
                <i class="fas fa-${this.getNotificationIcon(type)}"></i>
                <span>${message}</span>
                <button class="notification-close" onclick="this.parentElement.parentElement.remove()">
                    <i class="fas fa-times"></i>
                </button>
            </div>
        `;

        // Stilleri ekle
        notification.style.cssText = `
            position: fixed;
            top: 20px;
            right: 20px;
            background: ${this.getNotificationColor(type)};
            color: white;
            padding: 16px 20px;
            border-radius: 12px;
            box-shadow: 0 8px 25px rgba(0, 0, 0, 0.15);
            z-index: 1000;
            transform: translateX(100%);
            transition: transform 0.3s ease;
            max-width: 400px;
            font-weight: 500;
        `;

        notification.querySelector('.notification-content').style.cssText = `
            display: flex;
            align-items: center;
            gap: 12px;
        `;

        notification.querySelector('.notification-close').style.cssText = `
            background: rgba(255, 255, 255, 0.2);
            border: none;
            color: white;
            width: 24px;
            height: 24px;
            border-radius: 50%;
            cursor: pointer;
            display: flex;
            align-items: center;
            justify-content: center;
            margin-left: auto;
        `;

        document.body.appendChild(notification);

        // Ýçeri animasyonu
        setTimeout(() => {
            notification.style.transform = 'translateX(0)';
        }, 100);

        // 5 saniye sonra otomatik kaldýr
        setTimeout(() => {
            notification.style.transform = 'translateX(100%)';
            setTimeout(() => {
                if (notification.parentElement) {
                    notification.remove();
                }
            }, 300);
        }, 5000);
    }

    getNotificationIcon(type) {
        const icons = {
            success: 'check-circle',
            error: 'exclamation-circle',
            warning: 'exclamation-triangle',
            info: 'info-circle'
        };
        return icons[type] || icons.info;
    }

    getNotificationColor(type) {
        const colors = {
            success: 'linear-gradient(135deg, #10b981, #059669)',
            error: 'linear-gradient(135deg, #ef4444, #dc2626)',
            warning: 'linear-gradient(135deg, #f59e0b, #d97706)',
            info: 'linear-gradient(135deg, #6366f1, #3b82f6)'
        };
        return colors[type] || colors.info;
    }

    // Yardýmcý metodlar
    formatDate(date) {
        return new Intl.DateTimeFormat('tr-TR', {
            year: 'numeric',
            month: 'long',
            day: 'numeric'
        }).format(new Date(date));
    }

    formatFileSize(bytes) {
        if (bytes === 0) return '0 Bytes';
        const k = 1024;
        const sizes = ['Bytes', 'KB', 'MB', 'GB'];
        const i = Math.floor(Math.log(bytes) / Math.log(k));
        return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
    }

    // Panoya kopyalama iþlevselliði
    copyToClipboard(text) {
        navigator.clipboard.writeText(text).then(() => {
            this.showNotification('Panoya kopyalandý!', 'success');
        }).catch(() => {
            this.showNotification('Kopyalama baþarýsýz!', 'error');
        });
    }

    // Yazdýrma iþlevselliði
    printPage() {
        window.print();
    }

    // Paylaþma iþlevselliði
    sharePage() {
        if (navigator.share) {
            navigator.share({
                title: document.title,
                url: window.location.href
            });
        } else {
            this.copyToClipboard(window.location.href);
        }
    }
}

// DOM yüklendiðinde baþlat
document.addEventListener('DOMContentLoaded', () => {
    new ApprovalDetails();
});

// Potansiyel modül kullanýmý için dýþa aktar
if (typeof module !== 'undefined' && module.exports) {
    module.exports = ApprovalDetails;
}

// Bildirimler için ek CSS (dinamik olarak enjekte edilir)
const notificationStyles = `
    .notification {
        font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
    }
    
    .notification-close:hover {
        background: rgba(255, 255, 255, 0.3) !important;
    }
    
    @keyframes slideInRight {
        from {
            transform: translateX(100%);
            opacity: 0;
        }
        to {
            transform: translateX(0);
            opacity: 1;
        }
    }
    
    @keyframes slideOutRight {
        from {
            transform: translateX(0);
            opacity: 1;
        }
        to {
            transform: translateX(100%);
            opacity: 0;
        }
    }
    
    .fade-in-active {
        animation: fadeInUp 0.8s ease-out forwards;
    }
    
    @media (max-width: 480px) {
        .notification {
            right: 10px;
            left: 10px;
            max-width: none;
        }
    }
`;

// Bildirim stillerini enjekte et
const styleElement = document.createElement('style');
styleElement.textContent = notificationStyles;
document.head.appendChild(styleElement);
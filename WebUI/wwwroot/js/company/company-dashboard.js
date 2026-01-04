// Þirket Dashboard JavaScript

$(document).ready(function() {
    // Dashboard'u baþlat
    initializeDashboard();
    
    // Durum badge týklama iþleyicileri
    $('.status-badge').on('click', function(e) {
        e.stopPropagation();
        var status = $(this).data('status');
        if (status) {
            filterByStatus(status);
        }
    });
    
    // Baþvuru öðesi týklama iþleyicileri
    $('.application-item').on('click', function() {
        var applicationId = $(this).data('application-id');
        if (applicationId) {
            viewApplicationDetail(applicationId);
        }
    });
    
    // Her 30 saniyede veriyi yenile
    setInterval(function() {
        refreshDashboardData();
    }, 30000);
});

function initializeDashboard() {
    // Chart.js mevcutsa grafikleri baþlat
    if (typeof Chart !== 'undefined') {
        initializeCharts();
    }
    
    // Tooltip'leri baþlat
    initializeTooltips();
    
    // Bildirimleri baþlat
    checkForNewApplications();
}

function initializeCharts() {
    // Baþvuru Durumu Grafiði
    var statusCtx = document.getElementById('statusChart');
    if (statusCtx) {
        new Chart(statusCtx, {
            type: 'doughnut',
            data: {
                labels: ['Beklemede', 'Ýnceleniyor', 'Onaylandý', 'Reddedildi'],
                datasets: [{
                    data: [12, 8, 15, 5],
                    backgroundColor: [
                        '#fbbf24',
                        '#3b82f6',
                        '#10b981',
                        '#ef4444'
                    ]
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        position: 'bottom'
                    }
                }
            }
        });
    }
    
    // Zaman Ýçinde Baþvurular Grafiði
    var timeCtx = document.getElementById('applicationsChart');
    if (timeCtx) {
        new Chart(timeCtx, {
            type: 'line',
            data: {
                labels: ['Ocak', 'Þubat', 'Mart', 'Nisan', 'Mayýs', 'Haziran'],
                datasets: [{
                    label: 'Baþvuru Sayýsý',
                    data: [12, 19, 15, 25, 22, 30],
                    borderColor: '#3b82f6',
                    backgroundColor: 'rgba(59, 130, 246, 0.1)',
                    tension: 0.4
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                scales: {
                    y: {
                        beginAtZero: true
                    }
                }
            }
        });
    }
}

function initializeTooltips() {
    // Etkileþimli elementler için tooltip ekle
    $('[data-tooltip]').each(function() {
        var $element = $(this);
        var tooltipText = $element.data('tooltip');
        
        $element.on('mouseenter', function() {
            showTooltip($element, tooltipText);
        });
        
        $element.on('mouseleave', function() {
            hideTooltip();
        });
    });
}

function showTooltip($element, text) {
    var $tooltip = $('<div class="company-tooltip">' + text + '</div>');
    $('body').append($tooltip);
    
    var offset = $element.offset();
    $tooltip.css({
        top: offset.top - $tooltip.outerHeight() - 10,
        left: offset.left + ($element.outerWidth() / 2) - ($tooltip.outerWidth() / 2)
    });
    
    $tooltip.fadeIn(200);
}

function hideTooltip() {
    $('.company-tooltip').fadeOut(200, function() {
        $(this).remove();
    });
}

function filterByStatus(status) {
    // Durum filtresiyle baþvurular sayfasýna yönlendir
    window.location.href = '/Company/Applications?statusFilter=' + status;
}

function viewApplicationDetail(applicationId) {
    // Baþvuru detay sayfasýna git
    window.location.href = '/Company/ApplicationDetail/' + applicationId;
}

function refreshDashboardData() {
    // Dashboard istatistiklerini yenile (AJAX ile uygulanabilir)
    console.log('Dashboard verileri yenileniyor...');
}

function checkForNewApplications() {
    // Yeni baþvurularý kontrol et ve bildirim göster
    // SignalR ile gerçek zamanlý güncellemeler için uygulanabilir
    console.log('Yeni baþvurular kontrol ediliyor...');
}

// Global eriþim için fonksiyonlarý dýþa aktar
window.CompanyDashboard = {
    filterByStatus: filterByStatus,
    viewApplicationDetail: viewApplicationDetail,
    refreshDashboardData: refreshDashboardData
};

// Yardýmcý fonksiyonlar
function formatDate(dateString) {
    var date = new Date(dateString);
    return date.toLocaleDateString('tr-TR', {
        year: 'numeric',
        month: 'short',
        day: 'numeric'
    });
}

function formatCurrency(amount) {
    return new Intl.NumberFormat('tr-TR', {
        style: 'currency',
        currency: 'TRY'
    }).format(amount);
}

function showNotification(message, type = 'info') {
    var alertClass = 'alert-info';
    var icon = 'fas fa-info-circle';
    
    switch(type) {
        case 'success':
            alertClass = 'alert-success';
            icon = 'fas fa-check-circle';
            break;
        case 'error':
            alertClass = 'alert-danger';
            icon = 'fas fa-exclamation-circle';
            break;
        case 'warning':
            alertClass = 'alert-warning';
            icon = 'fas fa-exclamation-triangle';
            break;
    }
    
    var notification = `
        <div class="alert ${alertClass} alert-dismissible fade show" role="alert">
            <i class="${icon} me-2"></i>
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        </div>
    `;
    
    $('#notifications-container').append(notification);
    
    // 5 saniye sonra otomatik gizle
    setTimeout(function() {
        $('.alert').fadeOut();
    }, 5000);
}
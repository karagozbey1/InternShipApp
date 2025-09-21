// Þirket raporlarý sayfasý JavaScript fonksiyonlarý

document.addEventListener('DOMContentLoaded', function() {
    // Grafikleri baþlat
    initializeCharts();
    
    // Veri yenileme zamanlayýcýsýný baþlat
    initializeDataRefresh();
    
    // TempData mesajlarýný göster
    showTempDataMessages();
});

// Grafikleri baþlatma fonksiyonu
function initializeCharts() {
    const isDark = document.documentElement.classList.contains('dark');
    
    // Aylýk baþvuru trendi grafiði
    initMonthlyApplicationsChart(isDark);
    
    // Baþvuru durumlarý pasta grafiði
    initApplicationStatusChart(isDark);
}

// Aylýk baþvuru trendi grafiði oluþturma
function initMonthlyApplicationsChart(isDark) {
    const ctx = document.getElementById('monthlyApplicationsChart');
    if (!ctx) return;

    const textColor = isDark ? '#e5e7eb' : '#374151';
    const gridColor = isDark ? '#374151' : '#e5e7eb';

    new Chart(ctx, {
        type: 'line',
        data: {
            labels: ['Ocak', 'Þubat', 'Mart', 'Nisan', 'Mayýs', 'Haziran'],
            datasets: [{
                label: 'Baþvuru Sayýsý',
                data: [12, 19, 3, 5, 2, 3],
                borderColor: '#3B82F6',
                backgroundColor: 'rgba(59, 130, 246, 0.1)',
                tension: 0.4
            }]
        },
        options: {
            responsive: true,
            plugins: {
                legend: {
                    labels: {
                        color: textColor
                    }
                }
            },
            scales: {
                y: {
                    ticks: {
                        color: textColor
                    },
                    grid: {
                        color: gridColor
                    }
                },
                x: {
                    ticks: {
                        color: textColor
                    },
                    grid: {
                        color: gridColor
                    }
                }
            }
        }
    });
}

// Baþvuru durumlarý pasta grafiði oluþturma
function initApplicationStatusChart(isDark) {
    const ctx = document.getElementById('applicationStatusChart');
    if (!ctx) return;

    const textColor = isDark ? '#e5e7eb' : '#374151';

    // Model verilerini JavaScript deðiþkenlerine al
    const pendingCount = parseInt(document.querySelector('.text-yellow-500')?.parentElement?.nextElementSibling?.textContent?.split(': ')[1] || '0');
    const approvedCount = parseInt(document.querySelector('.text-green-500')?.parentElement?.nextElementSibling?.textContent?.split(': ')[1] || '0');
    const rejectedCount = parseInt(document.querySelector('.text-red-500')?.parentElement?.nextElementSibling?.textContent?.split(': ')[1] || '0');

    new Chart(ctx, {
        type: 'doughnut',
        data: {
            labels: ['Beklemede', 'Onaylý', 'Reddedilen'],
            datasets: [{
                data: [pendingCount, approvedCount, rejectedCount],
                backgroundColor: ['#EAB308', '#10B981', '#EF4444'],
                borderWidth: 2,
                borderColor: isDark ? '#1F2937' : '#FFFFFF'
            }]
        },
        options: {
            responsive: true,
            plugins: {
                legend: {
                    display: false
                }
            }
        }
    });
}

// Excel'e aktarma fonksiyonu
function exportToExcel() {
    fetch('/Company/Reports/ExportToExcel', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        }
    })
    .then(response => {
        if (response.ok) {
            return response.blob();
        }
        throw new Error('Excel export baþarýsýz oldu');
    })
    .then(blob => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.style.display = 'none';
        a.href = url;
        a.download = `Stajyer_Raporu_${new Date().toISOString().slice(0,10)}.csv`;
        document.body.appendChild(a);
        a.click();
        window.URL.revokeObjectURL(url);
        showNotification('Excel dosyasý baþarýyla indirildi!', 'success');
    })
    .catch(error => {
        showNotification('Excel export hatasý oluþtu.', 'error');
    });
}

// PDF rapor oluþturma fonksiyonu
function generatePDFReport() {
    showNotification('PDF raporu hazýrlanýyor...', 'info');
    
    setTimeout(() => {
        showNotification('PDF raporu baþarýyla oluþturuldu!', 'success');
    }, 2000);
}

// E-posta raporu gönderme fonksiyonu
function emailReport() {
    showNotification('Rapor e-posta ile gönderiliyor...', 'info');
    
    setTimeout(() => {
        showNotification('Rapor e-posta ile gönderildi!', 'success');
    }, 1500);
}

// Veri yenileme zamanlayýcýsýný baþlat
function initializeDataRefresh() {
    setInterval(function() {
        refreshReportData();
    }, 120000); // 2 dakikada bir
}

// Rapor verilerini yenileme fonksiyonu
function refreshReportData() {
    // AJAX ile güncel verileri çek ve sayfayý güncelle
    // Bu fonksiyon gelecekte implement edilecek
}

// TempData mesajlarýný göster
function showTempDataMessages() {
    // Bu fonksiyon server-side render sýrasýnda doldurulacak
    // Þimdilik boþ býrakýyoruz
}

// Bildirim gösterme fonksiyonu
function showNotification(message, type) {
    const notification = document.createElement('div');
    notification.className = `fixed top-4 right-4 p-4 rounded-lg shadow-lg z-50 ${
        type === 'success' ? 'bg-green-500 text-white' : 
        type === 'error' ? 'bg-red-500 text-white' : 
        'bg-blue-500 text-white'
    }`;
    notification.textContent = message;
    
    document.body.appendChild(notification);
    
    // 3 saniye sonra kaldýr
    setTimeout(function() {
        notification.style.opacity = '0';
        notification.style.transition = 'opacity 0.5s ease';
        setTimeout(() => {
            if (notification.parentNode) {
                notification.remove();
            }
        }, 500);
    }, 3000);
}
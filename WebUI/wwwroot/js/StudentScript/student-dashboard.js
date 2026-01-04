// Dashboard Yardýmcý Fonksiyonlarý
class DashboardManager {
    constructor() {
        this.initializeComponents();
        this.bindEvents();
        this.startAutoRefresh();
    }

    initializeComponents() {
        this.initStatCards();
        this.initActivityCards();
        this.initCharts();
    }

    bindEvents() {
        this.bindStatCardClicks();
        this.bindActivityClicks();
        this.bindRefreshButtons();
    }

    // Stat kartlarý animasyonu
    initStatCards() {
        const statCards = document.querySelectorAll('.stat-card');
        statCards.forEach((card, index) => {
            card.style.animationDelay = `${index * 0.1}s`;
            card.classList.add('animate-fade-in');
        });
    }

    // Stat kartlarý týklama olaylarý
    bindStatCardClicks() {
        const statCards = document.querySelectorAll('.stat-card');
        statCards.forEach(card => {
            card.addEventListener('click', (e) => {
                const targetUrl = card.dataset.url;
                if (targetUrl) {
                    window.location.href = targetUrl;
                }
            });
        });
    }

    // Aktivite kartlarý etkileþimi
    initActivityCards() {
        const activityRows = document.querySelectorAll('.activity-row');
        
        activityRows.forEach(row => {
            row.addEventListener('click', function() {
                this.style.transform = 'scale(0.98)';
                setTimeout(() => {
                    this.style.transform = '';
                }, 150);
            });
            
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

    bindActivityClicks() {
        const activities = document.querySelectorAll('.activity-item');
        activities.forEach(activity => {
            activity.addEventListener('click', (e) => {
                const activityId = activity.dataset.activityId;
                const activityType = activity.dataset.activityType;
                this.handleActivityClick(activityId, activityType);
            });
        });
    }

    handleActivityClick(activityId, activityType) {
        // Aktivite türüne göre yönlendirme
        switch (activityType) {
            case 'application':
                window.location.href = `/Student/Application/Details/${activityId}`;
                break;
            case 'approval':
                window.location.href = `/Student/Approval/Details/${activityId}`;
                break;
            case 'document':
                window.location.href = `/Student/Document`;
                break;
            default:
                console.warn('Bilinmeyen aktivite türü:', activityType);
        }
    }

    // Grafik kurulumu
    initCharts() {
        this.initInternshipChart();
        this.initPerformanceChart();
    }

    initInternshipChart() {
        const chartContainer = document.getElementById('internshipChart');
        if (chartContainer && window.Chart) {
            const ctx = chartContainer.getContext('2d');
            
            // Grafik verilerini AJAX ile al
            this.loadChartData().then(data => {
                new Chart(ctx, {
                    type: 'line',
                    data: data,
                    options: {
                        responsive: true,
                        maintainAspectRatio: false,
                        plugins: {
                            legend: {
                                display: true,
                                position: 'top'
                            }
                        },
                        scales: {
                            y: {
                                beginAtZero: true,
                                ticks: {
                                    precision: 0
                                }
                            }
                        }
                    }
                });
            });
        }
    }

    async loadChartData() {
        try {
            const response = await fetch('/Student/Dashboard/GetChartData', {
                method: 'GET',
                headers: {
                    'X-Requested-With': 'XMLHttpRequest'
                }
            });
            
            if (response.ok) {
                return await response.json();
            }
        } catch (error) {
            console.error('Grafik veri yükleme hatasý:', error);
        }
        
        // Yedek veri
        return {
            labels: ['Ocak', 'Þubat', 'Mart', 'Nisan', 'Mayýs', 'Haziran'],
            datasets: [{
                label: 'Baþvurular',
                data: [0, 0, 0, 0, 0, 0],
                borderColor: 'rgb(59, 130, 246)',
                tension: 0.1
            }]
        };
    }

    // Otomatik yenileme
    startAutoRefresh() {
        if (window.location.hostname === 'localhost') {
            console.log('Otomatik yenileme aktif - her 5 dakikada yenilenir');
            
            setInterval(() => {
                this.refreshDashboardData();
            }, 5 * 60 * 1000); // 5 dakika
        }
    }

    // Dashboard verilerini AJAX ile güncelle
    async refreshDashboardData() {
        try {
            const response = await fetch(window.location.href, {
                method: 'GET',
                headers: {
                    'X-Requested-With': 'XMLHttpRequest'
                }
            });
            
            if (response.ok) {
                const html = await response.text();
                const parser = new DOMParser();
                const doc = parser.parseFromString(html, 'text/html');
                
                this.updateStatCards(doc);
                this.updateActivities(doc);
                
                console.log('Dashboard verileri güncellendi');
            }
        } catch (error) {
            console.error('Dashboard güncelleme hatasý:', error);
        }
    }

    // Stat kartlarýný güncelle
    updateStatCards(newDoc) {
        const currentStats = document.querySelectorAll('.stats .big');
        const newStats = newDoc.querySelectorAll('.stats .big');
        
        currentStats.forEach((stat, index) => {
            if (newStats[index] && stat.textContent !== newStats[index].textContent) {
                this.animateNumberChange(stat, newStats[index].textContent);
            }
        });
    }

    // Sayý deðiþim animasyonu
    animateNumberChange(element, newValue) {
        const currentValue = parseInt(element.textContent) || 0;
        const targetValue = parseInt(newValue) || 0;
        
        if (currentValue === targetValue) return;
        
        const increment = targetValue > currentValue ? 1 : -1;
        const duration = 1000;
        const steps = Math.abs(targetValue - currentValue);
        const stepDuration = duration / steps;
        
        let current = currentValue;
        const timer = setInterval(() => {
            current += increment;
            element.textContent = current;
            
            if (current === targetValue) {
                clearInterval(timer);
                element.classList.add('animate-pulse');
                setTimeout(() => element.classList.remove('animate-pulse'), 500);
            }
        }, stepDuration);
    }

    // Aktiviteleri güncelle
    updateActivities(newDoc) {
        const currentActivities = document.querySelector('.recent-activities');
        const newActivities = newDoc.querySelector('.recent-activities');
        
        if (currentActivities && newActivities) {
            currentActivities.innerHTML = newActivities.innerHTML;
            this.initActivityCards(); // Yeniden olay baðlayýcý
        }
    }

    // Yenileme butonlarý
    bindRefreshButtons() {
        const refreshButtons = document.querySelectorAll('.refresh-btn');
        refreshButtons.forEach(btn => {
            btn.addEventListener('click', (e) => {
                e.preventDefault();
                this.refreshDashboardData();
                
                // Buton animasyonu
                btn.classList.add('animate-spin');
                setTimeout(() => btn.classList.remove('animate-spin'), 1000);
            });
        });
    }

    // Performans grafiði
    initPerformanceChart() {
        const performanceChart = document.getElementById('performanceChart');
        if (performanceChart && window.Chart) {
            const ctx = performanceChart.getContext('2d');
            
            new Chart(ctx, {
                type: 'doughnut',
                data: {
                    labels: ['Tamamlanan', 'Devam Eden', 'Bekleyen'],
                    datasets: [{
                        data: [60, 30, 10],
                        backgroundColor: [
                            'rgb(34, 197, 94)',
                            'rgb(59, 130, 246)',
                            'rgb(251, 191, 36)'
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
    }
}

// AJAX Yardýmcý Fonksiyonlarý
class AjaxHelper {
    static async get(url) {
        try {
            const response = await fetch(url, {
                method: 'GET',
                headers: {
                    'X-Requested-With': 'XMLHttpRequest',
                    'Content-Type': 'application/json'
                }
            });
            
            if (response.ok) {
                return await response.json();
            }
            throw new Error(`HTTP hatasý! durum: ${response.status}`);
        } catch (error) {
            console.error('AJAX GET hatasý:', error);
            throw error;
        }
    }
    
    static async post(url, data) {
        try {
            const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
            
            const response = await fetch(url, {
                method: 'POST',
                headers: {
                    'X-Requested-With': 'XMLHttpRequest',
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': token
                },
                body: JSON.stringify(data)
            });
            
            if (response.ok) {
                return await response.json();
            }
            throw new Error(`HTTP hatasý! durum: ${response.status}`);
        } catch (error) {
            console.error('AJAX POST hatasý:', error);
            throw error;
        }
    }
}

// Toast Bildirim Sistemi
class ToastManager {
    static show(message, type = 'info', duration = 3000) {
        const toast = document.createElement('div');
        toast.className = `toast toast-${type}`;
        toast.innerHTML = `
            <div class="toast-content">
                <i class="fas fa-${this.getIcon(type)}"></i>
                <span>${message}</span>
            </div>
            <button class="toast-close">&times;</button>
        `;
        
        document.body.appendChild(toast);
        
        // Otomatik kaldýrma
        setTimeout(() => this.remove(toast), duration);
        
        // Manuel kaldýrma
        toast.querySelector('.toast-close').addEventListener('click', () => {
            this.remove(toast);
        });
    }
    
    static getIcon(type) {
        const icons = {
            success: 'check-circle',
            error: 'exclamation-circle',
            warning: 'exclamation-triangle',
            info: 'info-circle'
        };
        return icons[type] || icons.info;
    }
    
    static remove(toast) {
        toast.style.animation = 'slideOut 0.3s ease-in';
        setTimeout(() => {
            if (toast.parentNode) {
                toast.parentNode.removeChild(toast);
            }
        }, 300);
    }
}

// Sayfa yüklendiðinde baþlat
document.addEventListener('DOMContentLoaded', () => {
    new DashboardManager();
});

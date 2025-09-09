// Advisor Dashboard Management
class AdvisorDashboardManager {
    constructor() {
        this.initializeComponents();
        this.bindEvents();
        this.startDataRefresh();
    }

    initializeComponents() {
        this.initializeCharts();
        this.loadRecentData();
    }

    bindEvents() {
        this.bindStatCards();
        this.bindStudentActions();
        this.bindApprovalActions();
        this.bindFilters();
    }

    bindStatCards() {
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

    bindStudentActions() {
        // Student assignment actions
        const assignButtons = document.querySelectorAll('.assign-student-btn');
        assignButtons.forEach(btn => {
            btn.addEventListener('click', this.handleStudentAssignment.bind(this));
        });

        // Student detail views
        const viewButtons = document.querySelectorAll('.view-student-btn');
        viewButtons.forEach(btn => {
            btn.addEventListener('click', this.handleStudentView.bind(this));
        });
    }

    bindApprovalActions() {
        // Approval form actions
        const approveButtons = document.querySelectorAll('.approve-btn');
        const rejectButtons = document.querySelectorAll('.reject-btn');
        
        approveButtons.forEach(btn => {
            btn.addEventListener('click', this.handleApproval.bind(this));
        });
        
        rejectButtons.forEach(btn => {
            btn.addEventListener('click', this.handleRejection.bind(this));
        });
    }

    async handleStudentAssignment(e) {
        e.preventDefault();
        
        const btn = e.currentTarget;
        const studentId = btn.dataset.studentId;
        
        if (!confirm('Bu öðrenciyi kendinize atamak istediðinizden emin misiniz?')) {
            return;
        }
        
        try {
            const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
            
            if (!token) {
                this.showMessage('Güvenlik token bulunamadý. Sayfayý yenileyin.', 'error');
                return;
            }
            
            const response = await fetch('/Advisor/AdvisorStudent/AssignStudent', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'X-Requested-With': 'XMLHttpRequest',
                    'RequestVerificationToken': token
                },
                body: JSON.stringify({ studentId: studentId })
            });
            
            if (response.ok) {
                this.showMessage('Öðrenci baþarýyla atandý!', 'success');
                location.reload();
            } else {
                throw new Error('Network response was not ok');
            }
        } catch (error) {
            console.error('Student assignment error:', error);
            this.showMessage('Öðrenci atama sýrasýnda hata oluþtu!', 'error');
        }
    }

    handleStudentView(e) {
        e.preventDefault();
        
        const btn = e.currentTarget;
        const studentId = btn.dataset.studentId;
        
        if (studentId) {
            window.location.href = `/Advisor/AdvisorStudent/Details/${studentId}`;
        }
    }

    async handleApproval(e) {
        e.preventDefault();
        
        const btn = e.currentTarget;
        const formId = btn.dataset.formId;
        
        const notes = prompt('Onay notunuzu giriniz (isteðe baðlý):');
        
        try {
            const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
            
            if (!token) {
                this.showMessage('Güvenlik token bulunamadý. Sayfayý yenileyin.', 'error');
                return;
            }
            
            const response = await fetch('/Advisor/Approval/Approve', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'X-Requested-With': 'XMLHttpRequest',
                    'RequestVerificationToken': token
                },
                body: JSON.stringify({ 
                    formId: formId,
                    notes: notes || ''
                })
            });
            
            if (response.ok) {
                this.showMessage('Form baþarýyla onaylandý!', 'success');
                location.reload();
            } else {
                throw new Error('Network response was not ok');
            }
        } catch (error) {
            console.error('Approval error:', error);
            this.showMessage('Onaylama sýrasýnda hata oluþtu!', 'error');
        }
    }

    async handleRejection(e) {
        e.preventDefault();
        
        const btn = e.currentTarget;
        const formId = btn.dataset.formId;
        
        const reason = prompt('Ret nedeni giriniz (zorunlu):');
        
        if (!reason || reason.trim() === '') {
            this.showMessage('Ret nedeni girmeniz gereklidir!', 'warning');
            return;
        }
        
        try {
            const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
            
            if (!token) {
                this.showMessage('Güvenlik token bulunamadý. Sayfayý yenileyin.', 'error');
                return;
            }
            
            const response = await fetch('/Advisor/Approval/Reject', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'X-Requested-With': 'XMLHttpRequest',
                    'RequestVerificationToken': token
                },
                body: JSON.stringify({ 
                    formId: formId,
                    reason: reason.trim()
                })
            });
            
            if (response.ok) {
                this.showMessage('Form baþarýyla reddedildi!', 'success');
                location.reload();
            } else {
                throw new Error('Network response was not ok');
            }
        } catch (error) {
            console.error('Rejection error:', error);
            this.showMessage('Reddetme sýrasýnda hata oluþtu!', 'error');
        }
    }

    bindFilters() {
        const filterButtons = document.querySelectorAll('.filter-btn');
        filterButtons.forEach(btn => {
            btn.addEventListener('click', (e) => {
                e.preventDefault();
                
                const filterType = btn.dataset.filter;
                
                // Update active filter
                filterButtons.forEach(b => b.classList.remove('active'));
                btn.classList.add('active');
                
                // Apply filter
                this.applyFilter(filterType);
            });
        });
    }

    applyFilter(filterType) {
        const items = document.querySelectorAll('.filterable-item');
        
        items.forEach(item => {
            const itemType = item.dataset.type;
            
            if (filterType === 'all' || itemType === filterType) {
                item.style.display = 'block';
            } else {
                item.style.display = 'none';
            }
        });
    }

    initializeCharts() {
        this.initStudentsChart();
        this.initApprovalsChart();
    }

    initStudentsChart() {
        const chartContainer = document.getElementById('studentsChart');
        if (chartContainer && window.Chart) {
            const ctx = chartContainer.getContext('2d');
            
            new Chart(ctx, {
                type: 'doughnut',
                data: {
                    labels: ['Atanmýþ Öðrenci', 'Aktif Staj', 'Tamamlanan'],
                    datasets: [{
                        data: window.studentStatsData || [0, 0, 0],
                        backgroundColor: [
                            'rgb(59, 130, 246)',
                            'rgb(34, 197, 94)',
                            'rgb(156, 163, 175)'
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

    initApprovalsChart() {
        const chartContainer = document.getElementById('approvalsChart');
        if (chartContainer && window.Chart) {
            const ctx = chartContainer.getContext('2d');
            
            new Chart(ctx, {
                type: 'bar',
                data: {
                    labels: ['Bekleyen', 'Onaylanan', 'Reddedilen'],
                    datasets: [{
                        label: 'Onay Formlarý',
                        data: window.approvalStatsData || [0, 0, 0],
                        backgroundColor: [
                            'rgb(251, 191, 36)',
                            'rgb(34, 197, 94)',
                            'rgb(239, 68, 68)'
                        ]
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    plugins: {
                        legend: {
                            display: false
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
        }
    }

    async loadRecentData() {
        try {
            const response = await fetch('/Advisor/AdvisorDashboard/GetRecentActivities', {
                method: 'GET',
                headers: {
                    'X-Requested-With': 'XMLHttpRequest'
                }
            });
            
            if (response.ok) {
                const activities = await response.json();
                this.updateRecentActivitiesList(activities);
            }
        } catch (error) {
            console.error('Recent data loading error:', error);
        }
    }

    updateRecentActivitiesList(activities) {
        const container = document.getElementById('recentActivitiesList');
        if (!container) return;
        
        if (!activities || activities.length === 0) {
            container.innerHTML = '<p class="text-muted text-center py-3">Henüz aktivite bulunmamaktadýr.</p>';
            return;
        }
        
        const html = activities.map(activity => `
            <div class="activity-item border-bottom py-2">
                <div class="d-flex justify-content-between align-items-center">
                    <div>
                        <strong>${activity.title || 'Baþlýksýz'}</strong>
                        <small class="text-muted d-block">${activity.description || ''}</small>
                    </div>
                    <div class="text-end">
                        <span class="badge bg-${this.getActivityColor(activity.type)}">${activity.type || 'general'}</span>
                        <small class="text-muted d-block">${activity.date || ''}</small>
                    </div>
                </div>
            </div>
        `).join('');
        
        container.innerHTML = html;
    }

    getActivityColor(type) {
        const colorMap = {
            'approval': 'warning',
            'assignment': 'info',
            'completion': 'success',
            'message': 'primary'
        };
        return colorMap[type] || 'secondary';
    }

    startDataRefresh() {
        // Refresh every 5 minutes
        setInterval(() => {
            this.loadRecentData();
        }, 5 * 60 * 1000);
    }

    // Message display helper
    showMessage(message, type = 'info') {
        // Check if ToastManager exists, otherwise use alert
        if (typeof ToastManager !== 'undefined') {
            ToastManager.show(message, type);
        } else {
            alert(message);
        }
    }
}

// Global instance
let advisorDashboardManager;

// Initialize when page loads
document.addEventListener('DOMContentLoaded', () => {
    advisorDashboardManager = new AdvisorDashboardManager();
});
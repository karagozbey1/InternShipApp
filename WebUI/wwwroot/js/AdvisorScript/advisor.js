// Advisor Dashboard JavaScript Functions

class AdvisorDashboard {
    constructor() {
        this.init();
    }

    init() {
        this.initializeEventListeners();
        this.loadDashboardStats();
        this.initializeNotifications();
        this.initializeModals();
        this.initializeCharts();
    }

    initializeEventListeners() {
        // Application approval/rejection buttons
        document.addEventListener('click', (e) => {
            if (e.target.classList.contains('approve-application-btn')) {
                this.handleApplicationApproval(e.target);
            }
            if (e.target.classList.contains('reject-application-btn')) {
                this.handleApplicationRejection(e.target);
            }
            if (e.target.classList.contains('approve-diary-btn')) {
                this.handleDiaryApproval(e.target);
            }
            if (e.target.classList.contains('reject-diary-btn')) {
                this.handleDiaryRejection(e.target);
            }
        });

        // Search and filter functionality
        const searchInputs = document.querySelectorAll('.advisor-search-input');
        searchInputs.forEach(input => {
            input.addEventListener('input', (e) => {
                this.handleSearch(e.target);
            });
        });

        // Refresh buttons
        const refreshButtons = document.querySelectorAll('.advisor-refresh-btn');
        refreshButtons.forEach(btn => {
            btn.addEventListener('click', () => {
                this.refreshData();
            });
        });
    }

    async loadDashboardStats() {
        try {
            const response = await fetch('/AdvisorDashboard/GetStats');
            const stats = await response.json();
            
            if (!stats.error) {
                this.updateDashboardCounters(stats);
            }
        } catch (error) {
            console.error('Error loading dashboard stats:', error);
        }
    }

    updateDashboardCounters(stats) {
        // Update pending applications count
        const pendingAppsElement = document.getElementById('pendingApplicationsCount');
        if (pendingAppsElement) {
            this.animateCounter(pendingAppsElement, stats.pendingApplications);
        }

        // Update pending diaries count
        const pendingDiariesElement = document.getElementById('pendingDiariesCount');
        if (pendingDiariesElement) {
            this.animateCounter(pendingDiariesElement, stats.pendingDiaries);
        }

        // Update total students count
        const totalStudentsElement = document.getElementById('totalStudentsCount');
        if (totalStudentsElement) {
            this.animateCounter(totalStudentsElement, stats.totalStudents);
        }

        // Update completed internships count
        const completedInternshipsElement = document.getElementById('completedInternshipsCount');
        if (completedInternshipsElement) {
            this.animateCounter(completedInternshipsElement, stats.completedInternships);
        }
    }

    animateCounter(element, targetValue) {
        const currentValue = parseInt(element.textContent) || 0;
        const increment = targetValue > currentValue ? 1 : -1;
        const stepTime = Math.abs(Math.floor(200 / (targetValue - currentValue))) || 1;

        let current = currentValue;
        const timer = setInterval(() => {
            current += increment;
            element.textContent = current;
            
            if (current === targetValue) {
                clearInterval(timer);
                element.classList.add('advisor-pulse');
                setTimeout(() => {
                    element.classList.remove('advisor-pulse');
                }, 1000);
            }
        }, stepTime);
    }

    async handleApplicationApproval(button) {
        const applicationId = button.dataset.applicationId;
        const modal = document.getElementById('approveApplicationModal');
        
        if (modal) {
            // Set application ID in modal
            const modalApplicationId = modal.querySelector('#modalApplicationId');
            if (modalApplicationId) {
                modalApplicationId.value = applicationId;
            }
            
            // Show modal
            this.showModal('approveApplicationModal');
        } else {
            // Direct approval without modal
            await this.submitApplicationAction(applicationId, 'approve', '');
        }
    }

    async handleApplicationRejection(button) {
        const applicationId = button.dataset.applicationId;
        const modal = document.getElementById('rejectApplicationModal');
        
        if (modal) {
            // Set application ID in modal
            const modalApplicationId = modal.querySelector('#modalApplicationId');
            if (modalApplicationId) {
                modalApplicationId.value = applicationId;
            }
            
            // Show modal
            this.showModal('rejectApplicationModal');
        }
    }

    async submitApplicationAction(applicationId, action, comment) {
        try {
            const formData = new FormData();
            formData.append('ApplicationId', applicationId);
            formData.append('Action', action);
            formData.append('Comment', comment);
            formData.append('__RequestVerificationToken', this.getAntiForgeryToken());

            const url = action === 'approve' ? '/AdvisorApplication/ApproveApplication' : '/AdvisorApplication/RejectApplication';
            
            const response = await fetch(url, {
                method: 'POST',
                body: formData
            });

            const result = await response.json();
            
            if (result.success) {
                this.showNotification('success', result.message);
                this.refreshApplicationRow(applicationId, action);
                this.hideModal('approveApplicationModal');
                this.hideModal('rejectApplicationModal');
            } else {
                this.showNotification('error', result.message);
            }
        } catch (error) {
            this.showNotification('error', 'Ýþlem sýrasýnda hata oluþtu: ' + error.message);
        }
    }

    async handleDiaryApproval(button) {
        const entryId = button.dataset.entryId;
        const modal = document.getElementById('approveDiaryModal');
        
        if (modal) {
            // Set entry ID in modal
            const modalEntryId = modal.querySelector('#modalEntryId');
            if (modalEntryId) {
                modalEntryId.value = entryId;
            }
            
            // Show modal
            this.showModal('approveDiaryModal');
        } else {
            // Direct approval without modal
            await this.submitDiaryAction(entryId, 'approve', '');
        }
    }

    async handleDiaryRejection(button) {
        const entryId = button.dataset.entryId;
        const modal = document.getElementById('rejectDiaryModal');
        
        if (modal) {
            // Set entry ID in modal
            const modalEntryId = modal.querySelector('#modalEntryId');
            if (modalEntryId) {
                modalEntryId.value = entryId;
            }
            
            // Show modal
            this.showModal('rejectDiaryModal');
        }
    }

    async submitDiaryAction(entryId, action, feedback) {
        try {
            const formData = new FormData();
            formData.append('DiaryEntryId', entryId);
            formData.append('Action', action);
            formData.append('Feedback', feedback);
            formData.append('__RequestVerificationToken', this.getAntiForgeryToken());

            const url = action === 'approve' ? '/AdvisorDiary/ApproveEntry' : '/AdvisorDiary/RejectEntry';
            
            const response = await fetch(url, {
                method: 'POST',
                body: formData
            });

            const result = await response.json();
            
            if (result.success) {
                this.showNotification('success', result.message);
                this.refreshDiaryRow(entryId, action);
                this.hideModal('approveDiaryModal');
                this.hideModal('rejectDiaryModal');
            } else {
                this.showNotification('error', result.message);
            }
        } catch (error) {
            this.showNotification('error', 'Ýþlem sýrasýnda hata oluþtu: ' + error.message);
        }
    }

    refreshApplicationRow(applicationId, action) {
        const row = document.querySelector(`[data-application-id="${applicationId}"]`)?.closest('tr');
        if (row) {
            const statusCell = row.querySelector('.status-cell');
            const actionCell = row.querySelector('.action-cell');
            
            if (statusCell) {
                statusCell.innerHTML = action === 'approve' 
                    ? '<span class="advisor-badge advisor-badge-approved">Onaylandý</span>'
                    : '<span class="advisor-badge advisor-badge-rejected">Reddedildi</span>';
            }
            
            if (actionCell) {
                actionCell.innerHTML = '<span class="text-gray-500">Ýþlem Tamamlandý</span>';
            }
            
            row.classList.add('advisor-fade-in-up');
        }
    }

    refreshDiaryRow(entryId, action) {
        const row = document.querySelector(`[data-entry-id="${entryId}"]`)?.closest('tr');
        if (row) {
            const statusCell = row.querySelector('.status-cell');
            const actionCell = row.querySelector('.action-cell');
            
            if (statusCell) {
                statusCell.innerHTML = action === 'approve' 
                    ? '<span class="advisor-badge advisor-badge-approved">Onaylandý</span>'
                    : '<span class="advisor-badge advisor-badge-pending">Geri Dönüþ Yapýldý</span>';
            }
            
            if (actionCell && action === 'approve') {
                actionCell.innerHTML = '<span class="text-gray-500">Onaylandý</span>';
            }
            
            row.classList.add('advisor-fade-in-up');
        }
    }

    initializeModals() {
        // Application approval modal
        document.addEventListener('click', (e) => {
            if (e.target.id === 'submitApplicationApproval') {
                const modal = document.getElementById('approveApplicationModal');
                const applicationId = modal.querySelector('#modalApplicationId').value;
                const comment = modal.querySelector('#approvalComment').value;
                this.submitApplicationAction(applicationId, 'approve', comment);
            }
            
            if (e.target.id === 'submitApplicationRejection') {
                const modal = document.getElementById('rejectApplicationModal');
                const applicationId = modal.querySelector('#modalApplicationId').value;
                const comment = modal.querySelector('#rejectionComment').value;
                
                if (!comment.trim()) {
                    this.showNotification('warning', 'Red nedeni belirtilmelidir.');
                    return;
                }
                
                this.submitApplicationAction(applicationId, 'reject', comment);
            }
            
            if (e.target.id === 'submitDiaryApproval') {
                const modal = document.getElementById('approveDiaryModal');
                const entryId = modal.querySelector('#modalEntryId').value;
                const feedback = modal.querySelector('#approvalFeedback').value;
                this.submitDiaryAction(entryId, 'approve', feedback);
            }
            
            if (e.target.id === 'submitDiaryRejection') {
                const modal = document.getElementById('rejectDiaryModal');
                const entryId = modal.querySelector('#modalEntryId').value;
                const feedback = modal.querySelector('#rejectionFeedback').value;
                
                if (!feedback.trim()) {
                    this.showNotification('warning', 'Geri dönüþ nedeni belirtilmelidir.');
                    return;
                }
                
                this.submitDiaryAction(entryId, 'reject', feedback);
            }
            
            // Close modal buttons
            if (e.target.classList.contains('close-modal-btn')) {
                const modalId = e.target.dataset.modalId;
                this.hideModal(modalId);
            }
        });
    }

    showModal(modalId) {
        const modal = document.getElementById(modalId);
        if (modal) {
            modal.classList.remove('hidden');
            modal.classList.add('flex');
            document.body.style.overflow = 'hidden';
            
            // Focus on first input
            const firstInput = modal.querySelector('input, textarea');
            if (firstInput) {
                setTimeout(() => firstInput.focus(), 100);
            }
        }
    }

    hideModal(modalId) {
        const modal = document.getElementById(modalId);
        if (modal) {
            modal.classList.add('hidden');
            modal.classList.remove('flex');
            document.body.style.overflow = 'auto';
            
            // Clear form inputs
            const inputs = modal.querySelectorAll('input, textarea');
            inputs.forEach(input => {
                if (input.type !== 'hidden') {
                    input.value = '';
                }
            });
        }
    }

    initializeCharts() {
        // Application status chart
        const applicationChartElement = document.getElementById('applicationStatusChart');
        if (applicationChartElement) {
            this.createApplicationStatusChart(applicationChartElement);
        }

        // Monthly statistics chart
        const monthlyChartElement = document.getElementById('monthlyStatsChart');
        if (monthlyChartElement) {
            this.createMonthlyStatsChart(monthlyChartElement);
        }
    }

    createApplicationStatusChart(element) {
        const ctx = element.getContext('2d');
        const chartData = JSON.parse(element.dataset.chartData || '{}');

        new Chart(ctx, {
            type: 'doughnut',
            data: {
                labels: ['Beklemede', 'Onaylandý', 'Reddedildi'],
                datasets: [{
                    data: [
                        chartData.pending || 0,
                        chartData.approved || 0,
                        chartData.rejected || 0
                    ],
                    backgroundColor: ['#f59e0b', '#10b981', '#ef4444'],
                    borderWidth: 0
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        position: 'bottom',
                        labels: {
                            usePointStyle: true,
                            padding: 20
                        }
                    }
                }
            }
        });
    }

    createMonthlyStatsChart(element) {
        const ctx = element.getContext('2d');
        const chartData = JSON.parse(element.dataset.chartData || '{}');

        new Chart(ctx, {
            type: 'line',
            data: {
                labels: chartData.labels || [],
                datasets: [
                    {
                        label: 'Baþvurular',
                        data: chartData.applications || [],
                        borderColor: '#667eea',
                        backgroundColor: 'rgba(102, 126, 234, 0.1)',
                        tension: 0.4
                    },
                    {
                        label: 'Staj Defteri Giriþleri',
                        data: chartData.diaryEntries || [],
                        borderColor: '#764ba2',
                        backgroundColor: 'rgba(118, 75, 162, 0.1)',
                        tension: 0.4
                    }
                ]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                scales: {
                    y: {
                        beginAtZero: true
                    }
                },
                plugins: {
                    legend: {
                        position: 'top'
                    }
                }
            }
        });
    }

    handleSearch(input) {
        const searchTerm = input.value.toLowerCase();
        const tableRows = document.querySelectorAll('.searchable-row');
        
        tableRows.forEach(row => {
            const searchableText = row.textContent.toLowerCase();
            if (searchableText.includes(searchTerm)) {
                row.style.display = '';
            } else {
                row.style.display = 'none';
            }
        });
    }

    async refreshData() {
        this.showNotification('info', 'Veriler yenileniyor...');
        
        try {
            await this.loadDashboardStats();
            this.showNotification('success', 'Veriler baþarýyla yenilendi.');
        } catch (error) {
            this.showNotification('error', 'Veriler yenilenirken hata oluþtu.');
        }
    }

    initializeNotifications() {
        // Auto-hide notifications after 5 seconds
        const notifications = document.querySelectorAll('.advisor-notification');
        notifications.forEach(notification => {
            setTimeout(() => {
                notification.style.opacity = '0';
                setTimeout(() => {
                    notification.remove();
                }, 300);
            }, 5000);
        });
    }

    showNotification(type, message) {
        const notification = document.createElement('div');
        notification.className = `advisor-notification advisor-notification-${type}`;
        notification.innerHTML = `
            <div class="flex items-center justify-between">
                <span>${message}</span>
                <button onclick="this.parentElement.parentElement.remove()" class="ml-4 text-gray-500 hover:text-gray-700">
                    <i class="fas fa-times"></i>
                </button>
            </div>
        `;
        
        document.body.appendChild(notification);
        
        // Auto-hide after 5 seconds
        setTimeout(() => {
            notification.style.opacity = '0';
            setTimeout(() => {
                notification.remove();
            }, 300);
        }, 5000);
    }

    getAntiForgeryToken() {
        const token = document.querySelector('input[name="__RequestVerificationToken"]');
        return token ? token.value : '';
    }
}

// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    new AdvisorDashboard();
});

// Export for global access
window.AdvisorDashboard = AdvisorDashboard;
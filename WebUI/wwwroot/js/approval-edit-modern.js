/**
 * Modern Approval Edit JavaScript
 * Handles form interactions, document selection, and animations
 */

class ApprovalEdit {
    constructor() {
        this.selectedDocuments = new Set();
        this.isSubmitting = false;
        this.init();
    }

    init() {
        this.bindEvents();
        this.initAnimations();
        this.setupFormValidation();
        this.initDocumentSelection();
        this.handleNotifications();
    }

    bindEvents() {
        // Document selection
        document.addEventListener('click', (e) => {
            if (e.target.closest('.document-item')) {
                this.handleDocumentClick(e.target.closest('.document-item'));
            }
        });

        // Form submission
        const form = document.querySelector('form');
        if (form) {
            form.addEventListener('submit', (e) => this.handleFormSubmit(e));
        }

        // Button loading states
        document.addEventListener('click', (e) => {
            if (e.target.closest('.btn-primary')) {
                this.handleSubmitButton(e.target.closest('.btn-primary'));
            }
        });

        // Auto-save functionality (optional)
        const textarea = document.querySelector('#StudentNote');
        if (textarea) {
            let autoSaveTimeout;
            textarea.addEventListener('input', () => {
                clearTimeout(autoSaveTimeout);
                autoSaveTimeout = setTimeout(() => {
                    this.autoSave();
                }, 2000);
            });
        }

        // Keyboard shortcuts
        document.addEventListener('keydown', (e) => {
            if (e.ctrlKey || e.metaKey) {
                switch(e.key) {
                    case 's':
                        e.preventDefault();
                        this.submitForm();
                        break;
                    case 'Escape':
                        this.goBack();
                        break;
                }
            }
        });
    }

    handleDocumentClick(item) {
        const checkbox = item.querySelector('input[type="checkbox"]');
        if (!checkbox) return;

        // Toggle checkbox state
        checkbox.checked = !checkbox.checked;
        
        // Update visual state
        this.updateDocumentState(item, checkbox.checked);
        
        // Update selected documents set
        const docId = parseInt(checkbox.value);
        if (checkbox.checked) {
            this.selectedDocuments.add(docId);
        } else {
            this.selectedDocuments.delete(docId);
        }

        // Animate selection
        this.animateDocumentSelection(item, checkbox.checked);
        
        // Update counter
        this.updateDocumentCounter();
    }

    updateDocumentState(item, isSelected) {
        if (isSelected) {
            item.classList.add('selected');
            item.style.transform = 'scale(1.02)';
            setTimeout(() => {
                item.style.transform = '';
            }, 200);
        } else {
            item.classList.remove('selected');
        }
    }

    animateDocumentSelection(item, isSelected) {
        const icon = item.querySelector('.document-icon');
        if (icon) {
            icon.style.transform = isSelected ? 'scale(1.2) rotate(360deg)' : 'scale(1)';
            setTimeout(() => {
                icon.style.transform = '';
            }, 300);
        }

        // Add ripple effect
        this.createRippleEffect(item);
    }

    createRippleEffect(element) {
        const ripple = document.createElement('div');
        ripple.style.cssText = `
            position: absolute;
            background: rgba(99, 102, 241, 0.3);
            border-radius: 50%;
            pointer-events: none;
            transform: scale(0);
            animation: ripple 0.6s linear;
            top: 50%;
            left: 50%;
            width: 20px;
            height: 20px;
            margin: -10px 0 0 -10px;
        `;

        element.style.position = 'relative';
        element.appendChild(ripple);

        setTimeout(() => {
            ripple.remove();
        }, 600);
    }

    updateDocumentCounter() {
        const counter = document.querySelector('.document-counter');
        if (counter) {
            counter.textContent = `${this.selectedDocuments.size} belge seçildi`;
        }
    }

    initAnimations() {
        // Stagger animations for form sections
        const sections = document.querySelectorAll('.form-section');
        sections.forEach((section, index) => {
            section.style.animationDelay = `${index * 0.1}s`;
        });

        // Intersection Observer for scroll animations
        const observer = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    entry.target.classList.add('animate-in');
                }
            });
        }, { threshold: 0.1 });

        document.querySelectorAll('.form-section').forEach(section => {
            observer.observe(section);
        });

        // Parallax effect for header
        window.addEventListener('scroll', () => {
            const scrolled = window.pageYOffset;
            const header = document.querySelector('.page-header');
            if (header) {
                header.style.transform = `translateY(${scrolled * 0.5}px)`;
            }
        });
    }

    setupFormValidation() {
        const form = document.querySelector('form');
        if (!form) return;

        // Real-time validation
        const inputs = form.querySelectorAll('input, textarea, select');
        inputs.forEach(input => {
            input.addEventListener('blur', () => this.validateField(input));
            input.addEventListener('input', () => this.clearFieldError(input));
        });

        // Custom validation rules
        this.setupCustomValidation();
    }

    setupCustomValidation() {
        const textarea = document.querySelector('#StudentNote');
        if (textarea) {
            textarea.addEventListener('input', () => {
                const wordCount = textarea.value.trim().split(/\s+/).length;
                const minWords = 10;
                
                const feedback = document.querySelector('.word-count-feedback') || 
                    this.createWordCountFeedback(textarea);
                
                feedback.textContent = `${wordCount} kelime (en az ${minWords} kelime gerekli)`;
                
                if (wordCount < minWords && textarea.value.trim()) {
                    textarea.classList.add('is-invalid');
                    feedback.classList.add('text-warning');
                } else {
                    textarea.classList.remove('is-invalid');
                    feedback.classList.remove('text-warning');
                }
            });
        }
    }

    createWordCountFeedback(textarea) {
        const feedback = document.createElement('div');
        feedback.className = 'word-count-feedback small text-muted mt-1';
        textarea.parentNode.appendChild(feedback);
        return feedback;
    }

    validateField(field) {
        let isValid = true;
        const errorContainer = this.getOrCreateErrorContainer(field);

        // Clear previous errors
        errorContainer.innerHTML = '';
        field.classList.remove('is-invalid', 'is-valid');

        // Required field validation
        if (field.hasAttribute('required') && !field.value.trim()) {
            this.showFieldError(field, 'Bu alan zorunludur.');
            isValid = false;
        }

        // Specific validations
        if (field.id === 'StudentNote' && field.value.trim()) {
            const wordCount = field.value.trim().split(/\s+/).length;
            if (wordCount < 10) {
                this.showFieldError(field, 'En az 10 kelime yazmalýsýnýz.');
                isValid = false;
            }
        }

        // Mark field as valid if no errors
        if (isValid && field.value.trim()) {
            field.classList.add('is-valid');
        }

        return isValid;
    }

    showFieldError(field, message) {
        field.classList.add('is-invalid');
        const errorContainer = this.getOrCreateErrorContainer(field);
        errorContainer.innerHTML = `<div class="invalid-feedback">${message}</div>`;
    }

    clearFieldError(field) {
        field.classList.remove('is-invalid');
        const errorContainer = this.getOrCreateErrorContainer(field);
        errorContainer.innerHTML = '';
    }

    getOrCreateErrorContainer(field) {
        let container = field.parentNode.querySelector('.error-container');
        if (!container) {
            container = document.createElement('div');
            container.className = 'error-container';
            field.parentNode.appendChild(container);
        }
        return container;
    }

    handleFormSubmit(e) {
        e.preventDefault();
        
        if (this.isSubmitting) return;
        
        // Validate form
        if (!this.validateForm()) {
            this.showNotification('Lütfen form hatalarýný düzeltin.', 'error');
            return;
        }

        // Check if any documents selected
        if (this.selectedDocuments.size === 0) {
            this.showNotification('En az bir belge seçmelisiniz.', 'warning');
            return;
        }

        this.submitForm();
    }

    validateForm() {
        const form = document.querySelector('form');
        const inputs = form.querySelectorAll('input[required], textarea[required]');
        let isValid = true;

        inputs.forEach(input => {
            if (!this.validateField(input)) {
                isValid = false;
            }
        });

        return isValid;
    }

    async submitForm() {
        if (this.isSubmitting) return;

        this.isSubmitting = true;
        const submitBtn = document.querySelector('.btn-primary');
        
        try {
            // Show loading state
            this.setButtonLoading(submitBtn, true);
            
            // Get form data
            const formData = new FormData(document.querySelector('form'));
            
            // Add selected documents
            this.selectedDocuments.forEach(docId => {
                formData.append('SelectedDocumentIds', docId);
            });

            // Submit form
            const response = await fetch(window.location.pathname, {
                method: 'POST',
                body: formData
            });

            if (response.ok) {
                this.showNotification('Form baþarýyla güncellendi!', 'success');
                
                // Redirect after success
                setTimeout(() => {
                    const urlParams = new URLSearchParams(formData);
                    const id = urlParams.get('Id');
                    window.location.href = `/Student/Approval/Details/${id}`;
                }, 1500);
            } else {
                throw new Error('Form güncellenemedi.');
            }

        } catch (error) {
            this.showNotification(error.message || 'Bir hata oluþtu.', 'error');
        } finally {
            this.isSubmitting = false;
            this.setButtonLoading(submitBtn, false);
        }
    }

    handleSubmitButton(button) {
        if (!this.isSubmitting) {
            this.setButtonLoading(button, true);
            setTimeout(() => {
                if (!this.isSubmitting) {
                    this.setButtonLoading(button, false);
                }
            }, 1000);
        }
    }

    setButtonLoading(button, loading) {
        if (loading) {
            button.classList.add('loading');
            const originalText = button.innerHTML;
            button.dataset.originalText = originalText;
            button.innerHTML = '<i class="fas fa-spinner fa-spin"></i><span>Gönderiliyor...</span>';
            button.disabled = true;
        } else {
            button.classList.remove('loading');
            if (button.dataset.originalText) {
                button.innerHTML = button.dataset.originalText;
            }
            button.disabled = false;
        }
    }

    initDocumentSelection() {
        // Initialize selected documents from checkboxes
        const checkboxes = document.querySelectorAll('input[name="SelectedDocumentIds"]:checked');
        checkboxes.forEach(checkbox => {
            this.selectedDocuments.add(parseInt(checkbox.value));
            const item = checkbox.closest('.document-item');
            if (item) {
                item.classList.add('selected');
            }
        });

        this.updateDocumentCounter();
    }

    autoSave() {
        // Optional auto-save functionality
        const data = {
            studentNote: document.querySelector('#StudentNote')?.value || '',
            selectedDocuments: Array.from(this.selectedDocuments)
        };

        localStorage.setItem('approval-edit-draft', JSON.stringify(data));
        this.showNotification('Taslak otomatik kaydedildi', 'info', 2000);
    }

    loadAutoSave() {
        try {
            const saved = JSON.parse(localStorage.getItem('approval-edit-draft'));
            if (saved) {
                const textarea = document.querySelector('#StudentNote');
                if (textarea && !textarea.value) {
                    textarea.value = saved.studentNote;
                }
            }
        } catch (error) {
            console.log('Auto-save load failed:', error);
        }
    }

    goBack() {
        if (confirm('Deðiþiklikler kaydedilmedi. Çýkmak istediðinizden emin misiniz?')) {
            window.history.back();
        }
    }

    handleNotifications() {
        // Handle URL parameters for notifications
        const urlParams = new URLSearchParams(window.location.search);
        const success = urlParams.get('success');
        const error = urlParams.get('error');

        if (success) {
            this.showNotification(decodeURIComponent(success), 'success');
        }
        if (error) {
            this.showNotification(decodeURIComponent(error), 'error');
        }

        // Load auto-save if available
        this.loadAutoSave();
    }

    showNotification(message, type = 'info', duration = 5000) {
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

        this.styleNotification(notification, type);
        document.body.appendChild(notification);

        // Animate in
        setTimeout(() => {
            notification.style.transform = 'translateX(0)';
        }, 100);

        // Auto remove
        if (duration > 0) {
            setTimeout(() => {
                notification.style.transform = 'translateX(100%)';
                setTimeout(() => {
                    if (notification.parentElement) {
                        notification.remove();
                    }
                }, 300);
            }, duration);
        }
    }

    styleNotification(notification, type) {
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

        const content = notification.querySelector('.notification-content');
        content.style.cssText = `
            display: flex;
            align-items: center;
            gap: 12px;
        `;

        const closeBtn = notification.querySelector('.notification-close');
        closeBtn.style.cssText = `
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
            transition: background 0.3s ease;
        `;
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
}

// Add CSS animations
const editAnimationStyles = `
    @keyframes ripple {
        to {
            transform: scale(4);
            opacity: 0;
        }
    }
    
    .animate-in {
        animation: fadeInUp 0.6s ease-out forwards;
    }
    
    @media (max-width: 768px) {
        .notification {
            right: 10px;
            left: 10px;
            max-width: none;
        }
    }
`;

const styleElement = document.createElement('style');
styleElement.textContent = editAnimationStyles;
document.head.appendChild(styleElement);

// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    new ApprovalEdit();
});

// Export for potential module usage
if (typeof module !== 'undefined' && module.exports) {
    module.exports = ApprovalEdit;
}
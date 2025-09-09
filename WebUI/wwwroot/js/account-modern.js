// Modern Account Pages JavaScript - Simple and Clean

document.addEventListener('DOMContentLoaded', function() {
    // Apply modern layout class to body
    document.body.classList.add('modern-account-layout');
    
    // Add floating shapes to body
    addFloatingShapes();
    
    // Initialize form interactions
    initFormInteractions();
    
    // Initialize password toggle
    initPasswordToggle();
    
    // Initialize university dropdown
    initUniversityDropdown();
});

// Add floating shapes to body
function addFloatingShapes() {
    if (document.querySelector('.floating-shapes')) return;

    const shapesContainer = document.createElement('div');
    shapesContainer.className = 'floating-shapes';
    
    for (let i = 1; i <= 4; i++) {
        const shape = document.createElement('div');
        shape.className = `shape shape-${i}`;
        shapesContainer.appendChild(shape);
    }
    
    document.body.appendChild(shapesContainer);

    // Add mouse interaction to shapes
    const shapes = document.querySelectorAll('.shape');
    shapes.forEach((shape, index) => {
        shape.addEventListener('mouseenter', function() {
            this.style.animationPlayState = 'paused';
            this.style.transform = 'scale(1.2)';
        });

        shape.addEventListener('mouseleave', function() {
            this.style.animationPlayState = 'running';
            this.style.transform = 'scale(1)';
        });
    });
}

// Initialize form interactions
function initFormInteractions() {
    // Force text color for all inputs
    const allInputs = document.querySelectorAll('input, textarea, select');
    allInputs.forEach(input => {
        // Set initial color
        input.style.color = '#2d3748';
        input.style.webkitTextFillColor = '#2d3748';
        
        // Maintain color on all events
        ['focus', 'blur', 'input', 'change'].forEach(event => {
            input.addEventListener(event, function() {
                this.style.color = '#2d3748';
                this.style.webkitTextFillColor = '#2d3748';
            });
        });
    });

    // Enhanced form validation display
    const forms = document.querySelectorAll('form');
    forms.forEach(form => {
        form.addEventListener('submit', function(e) {
            // Add loading state to submit button
            const submitBtn = this.querySelector('button[type="submit"]');
            if (submitBtn) {
                submitBtn.innerHTML = '<span class="spinner-border spinner-border-sm" role="status"></span> Ýþleniyor...';
                submitBtn.disabled = true;
            }
        });
    });
}

// Password toggle functionality
function initPasswordToggle() {
    const toggleButtons = document.querySelectorAll('.toggle-password');
    toggleButtons.forEach(button => {
        button.addEventListener('click', function() {
            const targetSelector = this.getAttribute('toggle');
            const targetField = document.querySelector(targetSelector);
            
            if (targetField) {
                if (targetField.type === 'password') {
                    targetField.type = 'text';
                    this.classList.remove('fa-eye');
                    this.classList.add('fa-eye-slash');
                } else {
                    targetField.type = 'password';
                    this.classList.remove('fa-eye-slash');
                    this.classList.add('fa-eye');
                }
            }
        });
    });
}

// University dropdown functionality
function initUniversityDropdown() {
    const universityDisplay = document.getElementById('university-display');
    const universityList = document.getElementById('university-list');
    const selectedUniversityName = document.getElementById('selected-university-name');
    
    if (!universityDisplay || !universityList || !selectedUniversityName) return;

    const searchBox = universityList.querySelector('.search-box input');
    const universityItems = universityList.querySelectorAll('li');

    // Toggle dropdown
    universityDisplay.addEventListener('click', function() {
        universityList.classList.toggle('active');
    });

    // Select university
    universityItems.forEach(item => {
        item.addEventListener('click', function() {
            const universityName = this.getAttribute('data-name');
            universityDisplay.value = universityName;
            selectedUniversityName.value = universityName;
            universityList.classList.remove('active');
            
            // Trigger validation
            universityDisplay.dispatchEvent(new Event('change'));
        });
    });

    // Search functionality
    if (searchBox) {
        searchBox.addEventListener('input', function() {
            const searchTerm = this.value.toLowerCase();
            universityItems.forEach(item => {
                const universityName = item.textContent.toLowerCase();
                if (universityName.includes(searchTerm)) {
                    item.style.display = 'block';
                } else {
                    item.style.display = 'none';
                }
            });
        });
    }

    // Close dropdown when clicking outside
    document.addEventListener('click', function(e) {
        if (!universityDisplay.contains(e.target) && !universityList.contains(e.target)) {
            universityList.classList.remove('active');
        }
    });
}

// Auto-hide alerts after 5 seconds
setTimeout(() => {
    const alerts = document.querySelectorAll('.alert');
    alerts.forEach(alert => {
        if (alert) {
            alert.style.opacity = '0';
            alert.style.transform = 'translateY(-10px)';
            setTimeout(() => {
                alert.style.display = 'none';
            }, 300);
        }
    });
}, 5000);
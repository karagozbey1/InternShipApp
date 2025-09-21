// Staj defteri kartlarý sayfasý JavaScript fonksiyonlarý

document.addEventListener('DOMContentLoaded', function() {
    // Kart animasyonlarýný baþlat
    initializeCardAnimations();
    
    // Modal event listener'larý ekle
    initializeModalEvents();
    
    // Sayfa dýþý týklama olaylarý
    initializeClickOutsideEvents();
});

// Modal açma/kapatma fonksiyonlarý
function showCreateDiaryModal() {
    document.getElementById('createDiaryModal').classList.remove('hidden');
    document.body.style.overflow = 'hidden';
}

function closeCreateDiaryModal() {
    document.getElementById('createDiaryModal').classList.add('hidden');
    document.body.style.overflow = 'auto';
}

function openDiaryEntry(entryId) {
    fetch(`/Student/Diary/GetEntryDetail/${entryId}`)
        .then(response => response.text())
        .then(html => {
            document.getElementById('diaryDetailContent').innerHTML = html;
            document.getElementById('diaryDetailModal').classList.remove('hidden');
            document.body.style.overflow = 'hidden';
        })
        .catch(error => {
            console.error('Hata:', error);
            alert('Detay yüklenirken hata oluþtu.');
        });
}

function closeDiaryDetailModal() {
    document.getElementById('diaryDetailModal').classList.add('hidden');
    document.body.style.overflow = 'auto';
}

function editDiaryEntry(entryId) {
    alert('Düzenleme özelliði yakýnda eklenecek.');
}

function toggleDropdown(entryId) {
    const dropdown = document.getElementById(`dropdown-${entryId}`);
    
    // Diðer tüm dropdown'larý kapat
    document.querySelectorAll('[id^="dropdown-"]').forEach(d => {
        if (d !== dropdown) d.classList.add('hidden');
    });
    
    dropdown.classList.toggle('hidden');
}

// Filtreleme fonksiyonlarý
function filterByCategory(category) {
    const cards = document.querySelectorAll('.diary-card');
    const filters = document.querySelectorAll('.category-filter');
    
    // Aktif filter stilini güncelle
    filters.forEach(f => f.classList.remove('active', 'bg-blue-500', 'text-white'));
    document.querySelector(`[data-category="${category}"]`).classList.add('active', 'bg-blue-500', 'text-white');
    
    cards.forEach(card => {
        const cardCategory = card.dataset.category;
        const cardDate = new Date(card.dataset.date);
        const weekAgo = new Date();
        weekAgo.setDate(weekAgo.getDate() - 7);
        const isApproved = card.dataset.approved === 'true';
        
        let shouldShow = false;
        
        switch(category) {
            case 'all':
                shouldShow = true;
                break;
            case 'recent':
                shouldShow = cardDate >= weekAgo;
                break;
            case 'approved':
                shouldShow = isApproved;
                break;
            default:
                shouldShow = cardCategory === category;
        }
        
        if (shouldShow) {
            card.style.display = 'block';
            setTimeout(() => {
                card.style.opacity = '1';
                card.style.transform = 'translateY(0)';
            }, 10);
        } else {
            card.style.opacity = '0';
            card.style.transform = 'translateY(20px)';
            setTimeout(() => {
                card.style.display = 'none';
            }, 300);
        }
    });
}

// Onaylý baþvurudan defter oluþtur
function createDiaryFromCard(applicationId) {
    if (!confirm('Bu baþvuru için staj defteri oluþturmak istiyor musunuz?')) return;
    
    const form = document.createElement('form');
    form.method = 'post';
    form.action = '/Student/DiaryNotebook/Create';
    form.style.display = 'none';

    const inputs = [
        { name: 'Title', value: 'Staj Defteri' },
        { name: 'CompanyName', value: '' },
        { name: 'Department', value: '' },
        { name: 'InternshipStartDate', value: '' },
        { name: 'InternshipEndDate', value: '' },
        { name: 'ApplicationId', value: applicationId }
    ];

    inputs.forEach(i => {
        const inp = document.createElement('input');
        inp.type = 'hidden';
        inp.name = i.name;
        inp.value = i.value;
        form.appendChild(inp);
    });

    // Anti-forgery token ekle
    const anti = document.querySelector('input[name="__RequestVerificationToken"]');
    if (anti) {
        const t = document.createElement('input');
        t.type = 'hidden';
        t.name = '__RequestVerificationToken';
        t.value = anti.value;
        form.appendChild(t);
    }

    document.body.appendChild(form);
    form.submit();
}

// Kart animasyonlarýný baþlat
function initializeCardAnimations() {
    const cards = document.querySelectorAll('.diary-card');
    cards.forEach((card, index) => {
        card.style.animationDelay = `${index * 0.1}s`;
        card.classList.add('fade-in-up');
    });
}

// Modal event listener'larý
function initializeModalEvents() {
    // Modal dýþýna týklanýnca kapat
    document.getElementById('createDiaryModal')?.addEventListener('click', function(e) {
        if (e.target === this) closeCreateDiaryModal();
    });

    document.getElementById('diaryDetailModal')?.addEventListener('click', function(e) {
        if (e.target === this) closeDiaryDetailModal();
    });

    // Escape tuþu ile modal kapat
    document.addEventListener('keydown', function(e) {
        if (e.key === 'Escape') {
            closeCreateDiaryModal();
            closeDiaryDetailModal();
        }
    });
}

// Sayfa dýþý týklama olaylarý
function initializeClickOutsideEvents() {
    document.addEventListener('click', function(event) {
        if (!event.target.closest('.dropdown')) {
            document.querySelectorAll('[id^="dropdown-"]').forEach(d => {
                d.classList.add('hidden');
            });
        }
    });
}
// Onay formu gözden geçirme sayfasý JavaScript fonksiyonlarý

document.addEventListener('DOMContentLoaded', function() {
    setupFormSubmission();
});

function setupFormSubmission() {
    const form = document.getElementById('step4Form');
    const submitBtn = document.getElementById('submitStep4');
    const serverErrorContainer = document.getElementById('step4ServerError');
    if (!form || !submitBtn) return;

    form.addEventListener('submit', async function (e) {
        e.preventDefault(); // Hatalarý göstermek için AJAX kullan

        try {
            // Önceki hatayý temizle
            if (serverErrorContainer) {
                serverErrorContainer.style.display = 'none';
                serverErrorContainer.textContent = '';
            }

            // Butonda yükleniyor durumunu göster
            const origHtml = submitBtn.innerHTML;
            submitBtn.dataset.origHtml = origHtml;
            submitBtn.disabled = true;
            submitBtn.innerHTML = '<i class="fas fa-spinner fa-spin mr-2"></i> Gönderiliyor...';

            const url = form.action || window.location.href;
            const formData = new FormData(form);

            // Antiforgery token'ý header'a da ekle (bazý endpoint'ler bunu bekler)
            const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
            const headers = {};
            if (tokenInput) headers['RequestVerificationToken'] = tokenInput.value;

            const resp = await fetch(url, {
                method: 'POST',
                credentials: 'same-origin',
                headers,
                body: formData,
                redirect: 'follow'
            });

            // Sunucu yönlendirme yaptýysa (POST sonrasý baþarýlý yönlendirme), tarayýcýda takip et
            if (resp.redirected) {
                window.location.href = resp.url;
                return;
            }

            // Yönlendirme yapýlmadý: büyük olasýlýkla hatalarla ayný view döndürüldü. HTML'i ayrýþtýr ve sunucu mesajýný çýkar
            const text = await resp.text();
            // Sunucu tarafý hata elementini bulmaya çalýþ
            try {
                const parser = new DOMParser();
                const doc = parser.parseFromString(text, 'text/html');
                const serverMsg = doc.querySelector('#step4ServerError');
                if (serverMsg && serverMsg.textContent.trim().length > 0) {
                    const msg = serverMsg.textContent.trim();
                    // Satýr içi göster
                    if (serverErrorContainer) {
                        serverErrorContainer.textContent = msg;
                        serverErrorContainer.style.display = 'block';
                    }
                    // Toast göster
                    if (window.DashboardUtils && typeof window.DashboardUtils.showNotification === 'function') {
                        window.DashboardUtils.showNotification(msg, 'error', 6000);
                    } else {
                        alert(msg);
                    }
                } else if (!resp.ok) {
                    const statusMsg = `Sunucu hatasý: ${resp.status} ${resp.statusText}`;
                    if (window.DashboardUtils && typeof window.DashboardUtils.showNotification === 'function') {
                        window.DashboardUtils.showNotification(statusMsg, 'error', 6000);
                    } else {
                        alert(statusMsg);
                    }
                } else {
                    // Sunucu mesajý yok ve ok: sayfayý yenile
                    window.location.reload();
                }
            } catch (parseErr) {
                console.warn('Sunucu yanýtý ayrýþtýrýlamadý', parseErr);
                if (!resp.ok) {
                    const statusMsg = `Sunucu hatasý: ${resp.status} ${resp.statusText}`;
                    if (window.DashboardUtils && typeof window.DashboardUtils.showNotification === 'function') {
                        window.DashboardUtils.showNotification(statusMsg, 'error', 6000);
                    } else {
                        alert(statusMsg);
                    }
                } else {
                    window.location.reload();
                }
            }
        } catch (err) {
            console.error('Gönderim hatasý', err);
            const msg = err && err.message ? err.message : 'Gönderme sýrasýnda beklenmeyen bir hata oluþtu.';
            if (window.DashboardUtils && typeof window.DashboardUtils.showNotification === 'function') {
                window.DashboardUtils.showNotification(msg, 'error', 6000);
            } else {
                alert(msg);
            }
        } finally {
            // Butonu geri yükle
            try {
                submitBtn.disabled = false;
                // Orijinal html kaydedildiyse geri yükle; yoksa varsayýlaný ayarla
                if (submitBtn.dataset && submitBtn.dataset.origHtml) submitBtn.innerHTML = submitBtn.dataset.origHtml;
                else submitBtn.innerHTML = 'Baþvuruyu Gönder';
            } catch (e) { /* yoksay */ }
        }
    });
}
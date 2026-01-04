// Belge önizleme modalý ile onay detaylarý sayfasý için geliþmiþ etkileþimler
document.addEventListener('DOMContentLoaded', function(){
  // fade-in gözlemcisi
  const faders = document.querySelectorAll('.fade-in');
  const obs = new IntersectionObserver(entries =>{
    entries.forEach(e =>{
      if(e.isIntersecting) e.target.classList.add('visible');
    });
  }, { threshold: 0.15 });
  faders.forEach(f => obs.observe(f));

  // belge önizleme modal elementleri
  const modal = document.createElement('div');
  modal.id = 'doc-modal';
  modal.innerHTML = `
    <div class="doc-modal-content" role="dialog" aria-modal="true" aria-label="Belge Önizleme">
      <div class="doc-modal-header">
        <strong id="doc-title">Belge</strong>
        <div>
          <button class="doc-modal-close" aria-label="Kapat">×</button>
        </div>
      </div>
      <div class="doc-modal-body"><iframe class="doc-iframe" src="about:blank"></iframe></div>
    </div>`;
  document.body.appendChild(modal);

  const iframe = modal.querySelector('.doc-iframe');
  const titleEl = modal.querySelector('#doc-title');
  const closeBtn = modal.querySelector('.doc-modal-close');

  // Belgeyi modalda aç
  function openDoc(url, title){
    titleEl.textContent = title || 'Belge';
    iframe.src = url;
    modal.classList.add('open');
    // odaðý yakala
    closeBtn.focus();
  }

  function closeDoc(){
    iframe.src = 'about:blank';
    modal.classList.remove('open');
  }

  closeBtn.addEventListener('click', closeDoc);
  modal.addEventListener('click', function(e){ if(e.target === modal) closeDoc(); });
  document.addEventListener('keydown', function(e){ if(e.key === 'Escape') closeDoc(); });

  // belge baðlantýsý iþleyicileri
  document.querySelectorAll('.doc-view').forEach(a=>{
    a.setAttribute('role','button');
    a.setAttribute('tabindex','0');
    const href = a.getAttribute('href');
    const title = a.getAttribute('aria-label') || a.textContent || 'Belge';
    a.addEventListener('click', function(e){
      e.preventDefault();
      // dosya PDF ise modalda aç, deðilse yeni sekmede aç
      if(href && href.toLowerCase().endsWith('.pdf')){
        openDoc(href, title.replace('Belgeyi görüntüle: ', ''));
      } else if(href){
        window.open(href, '_blank');
      }
    });
    a.addEventListener('keydown', function(e){ if(e.key === 'Enter' || e.key === ' ') { e.preventDefault(); this.click(); } });
  });

  // mobil kenar çubuðu geçiþi
  const toggle = document.querySelector('.sidebar-toggle');
  const rightCol = document.querySelector('.right');
  if(toggle && rightCol){
    toggle.addEventListener('click', () => {
      rightCol.classList.toggle('collapsed');
      const expanded = !rightCol.classList.contains('collapsed');
      toggle.setAttribute('aria-expanded', expanded);
      // açýldýðýnda kenar çubuðundaki ilk etkileþimli öðeye odaklan
      if(expanded){
        const first = rightCol.querySelector('a, button, [tabindex]');
        if(first) first.focus();
      }
    });
  }

  // yapýþkan yedek hafif efekt
  const right = document.querySelector('.right');
  if(right && window.innerWidth > 900){
    const top = right.getBoundingClientRect().top + window.pageYOffset;
    window.addEventListener('scroll', ()=>{
      if(window.scrollY > top - 28) right.classList.add('is-stuck'); else right.classList.remove('is-stuck');
    });
  }

  // genel butonlar için klavye etkinleþtirmesi
  document.querySelectorAll('button, a').forEach(el=>{
    el.addEventListener('keydown', (e)=>{
      if(e.key === 'Enter') el.click();
    });
  });
});

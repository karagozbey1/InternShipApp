// Duyurular sayfasý scripti - Grid tasarim
// Global durum
let filters = new URLSearchParams(window.location.search);
const IS_AUTH = (window.__isAuthenticated === true) || (document.body.getAttribute('data-auth')==='true');
const IS_STUDENT = (window.__isStudent === true) || (document.body.getAttribute('data-student')==='true');
const USER_ID = document.body.getAttribute('data-user-id') || '0';

/******************** Yardimci Fonksiyonlar ********************/
function q(id){return document.getElementById(id);}

function esc(str){
  if (!str) return '';
  return str.toString()
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/"/g, '&quot;')
    .replace(/'/g, '&#x27;');
}

function fmtDate(d){ 
  try { 
    return new Date(d).toLocaleDateString('tr-TR', {
      day: '2-digit',
      month: '2-digit', 
      year: 'numeric'
    }); 
  } catch { 
    return '-'; 
  } 
}

/******************** Filtre serileþtirme ve yukleme ********************/
function serializeFilters(page){
  const form = q('filterForm'); if(!form) return new URLSearchParams();
  const fd = new FormData(form);
  if(page) fd.set('page', page); else if(!fd.get('page')) fd.set('page', filters.get('page')||'1');
  const p = new URLSearchParams();
  ['q','city','field','company','sort','page'].forEach(k=>{ const v = fd.get(k); if(v) p.set(k,v); });
  return p;
}

function getInitialServerData(){
  try {
    const el = document.getElementById('initialOffersData');
    if (!el) return null;
    const json = JSON.parse(el.textContent||'{}');
    if (!json || !json.items) return null;
    return json;
  } catch {
    return null;
  }
}

async function loadOffers(page){
  const params = serializeFilters(page);
  params.set('pageSize', q('offersRoot')?.dataset.pageSize || '12');
  const list = q('announcementList');
  if(list) list.innerHTML = '<div class="text-center p-5" style="grid-column: 1/-1;"><i class="fas fa-spinner fa-spin fa-2x text-primary"></i><p class="mt-3 text-muted">Yukleniyor...</p></div>';
  let usedFallback = false;
  
  try {
    const res = await fetch('/api/public/offers?'+params.toString(), { method:'GET' });
    if (!res.ok) throw new Error(`HTTP ${res.status}`);
    const json = await res.json();
    if (!json || !json.items || json.items.length === 0) {
      const serverData = getInitialServerData();
      if (serverData) { usedFallback = true; applyData(serverData); }
      else { list.innerHTML = '<div class="empty-state" style="grid-column: 1/-1;"><div class="empty-state-icon"><i class="fas fa-search"></i></div><h4>Kayit bulunamadi</h4></div>'; }
    } else {
      applyData(json);
    }
  } catch (error) {
    const serverData = getInitialServerData();
    if (serverData) { usedFallback = true; applyData(serverData); }
    else if(list) { list.innerHTML = '<div class="text-center p-5 text-danger" style="grid-column: 1/-1;"><i class="fas fa-exclamation-triangle fa-2x mb-3"></i><p>Veri alinamadi.</p></div>'; }
  }
  
  function applyData(json) {
    filters = params;
    history.replaceState({},'', '/Home/Announcements?'+params.toString()+(usedFallback?'&fallback=1':''));
    renderOfferList(json); renderPagination(json); updateSummary(json);
  }
}

/******************** Render Islemleri ********************/
function updateSummary(j){
  const rs = q('resultSummary'); if(rs) rs.textContent = 'Toplam '+(j.totalCount||j.items?.length||0)+' kayit';
  const ps = q('pageSummary'); if(ps) ps.textContent = 'Sayfa '+(j.page||1)+'/'+(j.totalPages||1);
}

function renderOfferList(j){
  const listEl = q('announcementList'); if(!listEl) return;
  const items = (j.items||[]).filter(o => o.isActive !== false);
  
  const html = items.map(it=>{
    const workType = it.workType || '-';
    const showDate = it.applicationDeadline ? fmtDate(it.applicationDeadline) : fmtDate(it.publishDate);
    
    return `<div class="offer-card">
      <div class="offer-card-header">
        <h3 class="offer-card-title">${esc(it.title)}</h3>
        <div class="offer-card-company">
          <i class="fas fa-building text-primary"></i>
          <span>${esc(it.companyName)}</span>
        </div>
        <div class="offer-card-meta">
          <i class="fas fa-map-marker-alt"></i>
          <span>${esc(it.city || '-')}</span>
          <span class="mx-2">•</span>
          <i class="far fa-calendar"></i>
          <span>Son: ${showDate}</span>
        </div>
      </div>
      <div class="offer-card-body">
        <div class="offer-card-tags">
          <span class="offer-tag position">${esc(it.position || it.field || '-')}</span>
          <span class="offer-tag type">${esc(workType)}</span>
          ${it.salary ? `<span class="offer-tag salary">${esc(it.salary)}</span>` : ''}
        </div>
        ${it.description ? `<p class="offer-card-description">${esc(it.description)}</p>` : ''}
      </div>
      <div class="offer-card-footer">
        <span class="offer-card-date">
          <i class="far fa-clock me-1"></i>Ilan: ${fmtDate(it.publishDate)}
        </span>
        <a href="/Student/Application/OfferDetails/${it.id}?source=home" class="btn-detail">
          <i class="fas fa-eye"></i>Detay
        </a>
      </div>
    </div>`;
  }).join('');
  
  listEl.innerHTML = html || '<div class="empty-state" style="grid-column: 1/-1;"><div class="empty-state-icon"><i class="fas fa-inbox"></i></div><h4>Kayit bulunamadi</h4></div>';
}

function renderPagination(j){
  const bar = q('paginationBar'); if(!bar) return;
  const totalPages = j.totalPages || 1;
  const cur = j.page || 1;
  if(totalPages <= 1){ bar.innerHTML=''; return; }
  
  let h='<ul class="pagination pagination-sm flex-wrap mb-0 justify-content-center">';
  h+=`<li class="page-item ${cur==1?'disabled':''}"><a class="page-link" href="#" onclick="annLoad(${cur-1});return false;">Onceki</a></li>`;
  for(let i=1;i<=totalPages;i++) h+=`<li class="page-item ${cur==i?"active":""}"><a class="page-link" href="#" onclick="annLoad(${i});return false;">${i}</a></li>`;
  h+=`<li class="page-item ${cur==totalPages?'disabled':''}"><a class="page-link" href="#" onclick="annLoad(${cur+1});return false;">Sonraki</a></li>`;
  h+='</ul>'; 
  bar.innerHTML = h;
}

function annLoad(page) {
  loadOffers(page);
}

// Global scope
window.loadOffers = loadOffers;
window.annLoad = annLoad;

/******************** Baslatma ********************/
window.addEventListener('DOMContentLoaded', () => {
  const applyBtn = q('applyFilters'); 
  if(applyBtn) applyBtn.addEventListener('click', ()=>loadOffers(1));
});

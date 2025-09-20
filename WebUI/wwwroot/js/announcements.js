// Announcements page script (updated with MVC fallback + no auto select + removed hero chips + application deadline meta)
// Global state
let filters = new URLSearchParams(window.location.search);
let selectedOfferId = null;
const IS_AUTH = (window.__isAuthenticated === true) || (document.body.getAttribute('data-auth')==='true');
const IS_STUDENT = (window.__isStudent === true) || (document.body.getAttribute('data-student')==='true');
const USER_ID = document.body.getAttribute('data-user-id') || '0';

/******************** Helpers ********************/
function q(id){return document.getElementById(id);}        // shorthand

// Enhanced Turkish character encoding
function esc(str){
  if (!str) return '';
  return str.toString()
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/"/g, '&quot;')
    .replace(/'/g, '&#x27;');
}

// Proper encoding for Turkish characters in JSON
function encQuotes(str){
  if (!str) return '';
  return str.replace(/'/g,'&#39;').replace(/"/g,'&quot;');
}

function decQuotes(str){
  if (!str) return '';
  const txt = str.replace(/&#39;/g,"'").replace(/&quot;/g,'"');
  // Ensure UTF-8 retained (no extra processing needed in modern browsers) just return
  return txt;
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

function splitLines(val){ return !val?[]:val.split(/\r?\n|[,;]+/).map(s=>s.trim()).filter(Boolean); }

// LocalStorage favorite management (consistent with OfferDetails page)
function getFavSet(){
  try{ 
    const storageKey = `favOffers:${USER_ID}`;
    const raw = localStorage.getItem(storageKey) || ''; 
    return new Set(raw.split(',').filter(x=>x)); 
  } catch { 
    return new Set(); 
  }
}

function saveFavSet(set){
  try{ 
    const storageKey = `favOffers:${USER_ID}`;
    localStorage.setItem(storageKey, Array.from(set).join(',')); 
    
    // Dispatch custom event for cross-page communication
    window.dispatchEvent(new CustomEvent('favoriteUpdated', {
      detail: { userId: USER_ID, favoriteCount: set.size }
    }));
    
    console.log('Dispatched favoriteUpdated event, count:', set.size);
  } catch {}
}

/******************** Filter serialize & load ********************/
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
  console.log('loadOffers called with page:', page);
  const params = serializeFilters(page);
  params.set('pageSize', q('offersRoot')?.dataset.pageSize || '10');
  const list = q('announcementList');
  if(list) list.innerHTML = '<div class="p-3 text-center text-muted small">Yükleniyor...</div>';
  let usedFallback = false;
  
  try {
    const res = await fetch('/api/public/offers?'+params.toString(), { method:'GET' });
    console.log('API response status:', res.status);
    if (!res.ok) {
      throw new Error(`HTTP ${res.status}: ${res.statusText}`);
    }
    const json = await res.json();
    console.log('API response data:', json);
    if (!json || !json.items || json.items.length === 0) {
      const serverData = getInitialServerData();
      if (serverData) {
        usedFallback = true;
        applyData(serverData);
      } else {
        list.innerHTML = '<div class="p-3 text-center text-muted small">Kayýt bulunamadý.</div>';
      }
    } else {
      applyData(json);
    }
  } catch (error) {
    console.error('API error:', error);
    const serverData = getInitialServerData();
    if (serverData) {
      usedFallback = true;
      applyData(serverData);
    } else if(list) {
      list.innerHTML = '<div class="p-3 text-center text-danger small">Veri alýnamadý.</div>';
    }
  }
  
  function applyData(json) {
    filters = params;
    history.replaceState({},'', '/Home/Announcements?'+params.toString()+(usedFallback?'&fallback=1':''));
    renderOfferList(json); renderPagination(json); updateSummary(json);
  }
}

/******************** Rendering ********************/
function updateSummary(j){
  const rs = q('resultSummary'); if(rs) rs.textContent = 'Toplam '+(j.totalCount||j.items?.length||0)+' kayýt';
  const ps = q('pageSummary'); if(ps) ps.textContent = 'Sayfa '+(j.page||1)+'/'+(j.totalPages||1);
}

function renderOfferList(j){
  console.log('renderOfferList called with:', j);
  const listEl = q('announcementList'); if(!listEl) return;
  const items = (j.items||[]).filter(o => o.isActive !== false);
  const favSet = getFavSet();
  
  const html = items.map(it=>{
    const isFavorite = favSet.has(String(it.id));
    return `<div class='offer-tile offer-tile-enhanced' data-json='${encQuotes(JSON.stringify(it))}' onclick='annSelect(this)'>
      <div class='d-flex justify-content-between mb-2'>
        <h6 class='mb-1 offer-title text-truncate fw-bold' title='${esc(it.title)}'>${esc(it.title)}</h6>
        <small class='text-muted'>${fmtDate(it.publishDate)}</small>
      </div>
      ${it.applicationDeadline?`<div class="small text-muted mb-1 d-flex align-items-center gap-1"><i class='far fa-calendar'></i><span>Son Baþvuru: ${fmtDate(it.applicationDeadline)}</span></div>`:''}
      <div class='small text-muted mb-2 text-truncate'>
        <i class='fas fa-building me-1 text-primary'></i>${esc(it.companyName)}
      </div>
      <div class='small mb-2'>
        <i class='fas fa-location-dot me-1 text-secondary'></i>
        <span class='text-muted'>${esc(it.city || '-')}</span>
      </div>
      
      <div class='offer-details mt-2 pt-2 border-top'>
        <div class='row g-1 small'>
          <div class='col-4 text-center'>
            <div class='detail-box bg-light rounded p-1'>
              <div class='text-primary fw-bold small'>${esc(it.position || it.field || '-')}</div>
              <div class='text-xs text-muted'>Pozisyon</div>
            </div>
          </div>
          <div class='col-4 text-center'>
            <div class='detail-box bg-light rounded p-1'>
              <div class='text-success fw-bold small'>${esc(it.workType || '-')}</div>
              <div class='text-xs text-muted'>Tür</div>
            </div>
          </div>
          <div class='col-4 text-center'>
            <div class='detail-box bg-light rounded p-1'>
              <div class='text-info fw-bold small'>${esc(it.salary || '-')}</div>
              <div class='text-xs text-muted'>Maaþ</div>
            </div>
          </div>
        </div>
      </div>
    </div>`;
  }).join('');
  listEl.innerHTML = html || '<div class="p-4 text-center text-muted">Kayýt bulunamadý.</div>';
}

function renderPagination(j){
  const bar = q('paginationBar'); if(!bar) return;
  const totalPages = j.totalPages || 1;
  const cur = j.page || 1;
  if(totalPages <= 1){ bar.innerHTML=''; return; }
  let h='<ul class="pagination pagination-sm flex-wrap mb-0 justify-content-center">';
  h+=`<li class='page-item ${cur==1?'disabled':''}'><a class='page-link' href='#' onclick='annLoad(${cur-1});return false;'>Önceki</a></li>`;
  for(let i=1;i<=totalPages;i++) h+=`<li class='page-item ${cur==i?"active":""}'><a class='page-link' href='#' onclick='annLoad(${i});return false;'>${i}</a></li>`;
  h+=`<li class='page-item ${cur==totalPages?'disabled':''}'><a class='page-link' href='#' onclick='annLoad(${cur+1});return false;'>Sonraki</a></li>`;
  h+='</ul>'; bar.innerHTML = h;
}

/******************** Selection ********************/
function annSelect(tile){
  console.log('annSelect called with tile:', tile);
  if (!tile) { console.error('Tile is null or undefined'); return; }
  document.querySelectorAll('.offer-tile.active').forEach(x=>x.classList.remove('active'));
  tile.classList.add('active');
  let data; 
  try { 
    const jsonStr = tile.getAttribute('data-json');
    console.log('Raw data-json attribute:', jsonStr);
    if (!jsonStr) { console.error('No data-json attribute found on tile'); return; }
    data = JSON.parse(decQuotes(jsonStr));
    console.log('Parsed data:', data);
  } catch (error) { 
    console.error('JSON parse error:', error); console.error('Failed to parse JSON string:', tile.getAttribute('data-json')); return; 
  }
  selectedOfferId = data.id; console.log('Selected offer ID:', selectedOfferId);
  
  const defaultState = q('defaultState');
  const detailHero = q('detailHero');
  const mainContent = q('mainContent');
  
  if (defaultState){
    defaultState.style.display='none';
    try { defaultState.parentElement && defaultState.parentElement.removeChild(defaultState); } catch(e){ console.warn('defaultState remove failed', e); }
    console.log('Placeholder removed');
  }
  if (detailHero){ detailHero.style.display='block'; }
  if (mainContent){ mainContent.style.display='flex'; }
  
  const titleEl = q('detailTitle'); const metaEl = q('detailMeta');
  if (titleEl){ titleEl.innerText = data.title || 'Ýlan Baþlýðý'; }
  const dateForMeta = data.applicationDeadline || data.publishDate; 
  if (metaEl){ metaEl.textContent = `${data.companyName || ''} • ${data.city || ''} • ${fmtDate(dateForMeta)}`; }
  
  const btn = q('applyBtn'); if(btn){ btn.classList.remove('d-none'); btn.disabled=false; }
  
  const descContentEl = q('descContent'); 
  if (descContentEl){ 
    descContentEl.innerHTML = data.description ? `<p class="mb-0">${esc(data.description)}</p>` : '<p class="text-muted mb-0">Açýklama bulunmamaktadýr.</p>'; 
  }
  
  const reqItems = splitLines(data.requirements).map(r=>`<li class="mb-2"><i class="fas fa-check-circle text-success me-2"></i>${esc(r)}</li>`).join('');
  const reqContentEl = q('requirementsContent'); 
  if (reqContentEl){ 
    reqContentEl.innerHTML = reqItems ? `<ul class="list-unstyled mb-0">${reqItems}</ul>` : '<p class="text-muted mb-0">Gereksinim bilgisi belirtilmemiþ.</p>'; 
  }
  
  // Company logo
  const desiredInitial = (data.companyName && data.companyName.trim().length>0 ? data.companyName.trim()[0] : (data.title && data.title.trim().length>0 ? data.title.trim()[0] : '')).toUpperCase();
  const logoEl = q('companyLogo');
  if (logoEl){
    if(desiredInitial){ logoEl.textContent = desiredInitial; logoEl.title = data.companyName || data.title || ''; logoEl.classList.remove('d-none'); }
    else { logoEl.textContent=''; logoEl.classList.add('d-none'); }
  }
  
  const nameEl = q('companyName'); if (nameEl){ nameEl.textContent = data.companyName || 'Firma Adý'; }
  const locationEl = q('companyLocation'); 
  if (locationEl){ 
    locationEl.innerHTML = data.city ? `<i class="fas fa-map-marker-alt me-1"></i>${esc(data.city)}` : '<i class="fas fa-map-marker-alt me-1"></i>Konum bilgisi yok'; 
  }
  
  const cw = q('companyWebsite'); 
  if (cw){ 
    let wHtml=''; 
    if(data.website){ 
      let url = data.website.startsWith('http')?data.website:'https://'+data.website; 
      wHtml += `<a href='${esc(url)}' target='_blank' class='d-inline-block me-2'><i class="fas fa-globe me-1"></i>Site</a>`; 
    } 
    if(data.email){ 
      wHtml += `<a href='mailto:${esc(data.email)}' class='d-inline-block'><i class="fas fa-envelope me-1"></i>E-posta</a>`; 
    } 
    cw.innerHTML = wHtml; 
  }
  
  let cHtml=''; 
  if(data.phone){ 
    cHtml += `<div class='mb-2'><i class='fas fa-phone me-2 text-primary'></i><a href='tel:${esc(data.phone)}'>${esc(data.phone)}</a></div>`; 
  } 
  if(data.address){ 
    cHtml += `<div class='mb-2'><i class='fas fa-location-dot me-2 text-primary'></i><span>${esc(data.address)}</span></div>`; 
  }
  q('companyContent').innerHTML = cHtml || '<span class="text-muted">Ýletiþim bilgisi yok</span>';
  
  let about='';
  if(data.companyDescription && data.companyDescription.trim().length>0){
    about = esc(data.companyDescription);
  }
  q('companyAbout').textContent = about;
  
  let extra=[];
  if(data.establishedDate){
    try{ 
      const d=new Date(data.establishedDate); 
      extra.push(`<span class='me-3'><i class='fas fa-calendar-alt me-1 text-secondary'></i>Kuruluþ: ${d.toLocaleDateString('tr-TR')}</span>`);
    } catch(e) {
      console.warn('establishedDate parse',e);
    }
  }
  if(data.employeeCount){
    extra.push(`<span class='me-3'><i class='fas fa-users me-1 text-secondary'></i>Çalýþan: ${esc(data.employeeCount)}</span>`);
  }
  q('companyExtra').innerHTML = extra.join('');
  
  // Update save button state
  updateSaveButtonState();
}

function updateSaveButtonState() {
  if (!selectedOfferId) return;
  
  const saveBtn = q('saveBtn');
  if (!saveBtn) return;
  
  const favSet = getFavSet();
  const isFavorite = favSet.has(String(selectedOfferId));
  
  saveBtn.innerHTML = isFavorite 
    ? '<i class="fas fa-bookmark me-1"></i>Kaydedildi'
    : '<i class="fas fa-bookmark me-1"></i>Kaydet';
  saveBtn.className = isFavorite 
    ? 'btn btn-warning'
    : 'btn btn-light';
}

function annApply(){
  console.log('annApply called, selectedOfferId:', selectedOfferId);
  if(!selectedOfferId) return;
  
  if(!IS_AUTH){ 
    const currentUrl = window.location.pathname + window.location.search;
    const returnUrl = encodeURIComponent(currentUrl + (currentUrl.includes('?') ? '&' : '?') + 'selectedOffer=' + selectedOfferId);
    location.href='/Account/Login?returnUrl=' + returnUrl;
    return; 
  }
  
  if(!IS_STUDENT){ 
    alert('Baþvuru yalnýzca öðrenci kullanýcýlarý ile mümkündür.'); 
    return; 
  }
  
  location.href = '/Student/Application/Create?offerId=' + selectedOfferId;
}

function annSave(){
  console.log('annSave called, selectedOfferId:', selectedOfferId);
  if(!selectedOfferId) return;
  
  if(!IS_AUTH){ 
    const currentUrl = window.location.pathname + window.location.search;
    const returnUrl = encodeURIComponent(currentUrl + (currentUrl.includes('?') ? '&' : '?') + 'selectedOffer=' + selectedOfferId);
    location.href='/Account/Login?returnUrl=' + returnUrl;
    return; 
  }
  
  if(!IS_STUDENT){
    alert('Favorilere ekleme yalnýzca öðrenci kullanýcýlarý ile mümkündür.');
    return;
  }
  
  // Update localStorage immediately
  const favSet = getFavSet();
  let willBeFavorite;
  
  if(favSet.has(String(selectedOfferId))) {
    favSet.delete(String(selectedOfferId));
    willBeFavorite = false;
  } else {
    favSet.add(String(selectedOfferId));
    willBeFavorite = true;
  }
  
  saveFavSet(favSet);
  updateSaveButtonState();
  
  console.log(`Favorite ${willBeFavorite ? 'added' : 'removed'} for offer ${selectedOfferId}, total favorites: ${favSet.size}`);
  
  // Sync with server
  fetch(`/api/public/offers/${selectedOfferId}/favorite`, { method:'POST' })
    .then(r => {
      if (!r.ok) {
        console.warn('Server sync failed:', r.status);
      }
      return r.json();
    })
    .then(j => { 
      console.log('Server sync response:', j);
    })
    .catch(error => {
      console.error('Server sync error:', error);
    });
}

function annLoad(page) {
  console.log('annLoad called with page:', page);
  loadOffers(page);
}

// Expose functions to global scope IMMEDIATELY (before DOM loads)
window.loadOffers = loadOffers;
window.annSelect = annSelect;
window.annApply = annApply;
window.annSave = annSave;
window.annLoad = annLoad;

console.log('Functions exposed to window with Turkish character support:', {
  loadOffers: typeof window.loadOffers,
  annSelect: typeof window.annSelect,
  annApply: typeof window.annApply,
  annSave: typeof window.annSave,
  annLoad: typeof window.annLoad
});

/******************** Init ********************/
window.addEventListener('DOMContentLoaded', () => {
  console.log('DOM Content Loaded - Enhanced Layout with Turkish Support');
  console.log('IS_AUTH:', IS_AUTH, 'IS_STUDENT:', IS_STUDENT);
  
  // Add save button event handler
  const saveBtn = q('saveBtn');
  if(saveBtn) {
    saveBtn.addEventListener('click', annSave);
    console.log('Save button event added');
  }
  
  // Check if elements exist
  console.log('Checking required elements...');
  const elementsToCheck = ['offersRoot', 'announcementList', 'defaultState', 'detailHero', 'mainContent'];
  elementsToCheck.forEach(id => {
    const el = q(id);
    console.log(`Element ${id}:`, el ? 'EXISTS' : 'MISSING', el);
  });
  
  const applyBtn = q('applyFilters'); 
  if(applyBtn) {
    applyBtn.addEventListener('click', ()=>loadOffers(1));
    console.log('Filter button event added');
  } else {
    console.warn('Apply filters button not found');
  }
});

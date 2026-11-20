'use strict';
(() => {
    const assets = window.ASSET_CATALOG || [];
    const $ = id => document.getElementById(id);
    const groups = { '01_Player': 'Nhân vật chính', '02_Enemies': 'Quái vật', '03_Bosses': 'Boss', '04_Bullets': 'Đạn phép', '05_Skills': 'Kỹ năng Q · E · R', '06_Backgrounds': 'Bối cảnh', '07_UI': 'Giao diện', '08_Placeholders': 'Placeholder', '09_Effects': 'Hiệu ứng', '10_Unity': 'Tài nguyên Unity', '11_Concepts': 'Concept', '12_Previews': 'Ảnh gameplay', '13_Documentation': 'Tài liệu & prompt' };
    const storageKey = 'houkimajo.rawassets.selection.v1';
    let selected = new Set();
    try { const saved = JSON.parse(localStorage.getItem(storageKey) || '[]'); if (Array.isArray(saved)) selected = new Set(saved.filter(item => assets.some(asset => asset.path === item))); } catch { /* Vẫn chọn được trong phiên khi trình duyệt chặn lưu cục bộ. */ }
    let group = '', current = null, timer = null, toastTimer;
    const normalize = text => text.normalize('NFD').replace(/[\u0300-\u036f]/g, '').replaceAll('đ', 'd').toLowerCase();
    const url = file => file.split('/').map(encodeURIComponent).join('/');
    const size = bytes => bytes < 1024 ? `${bytes} B` : bytes < 1048576 ? `${(bytes / 1024).toFixed(1)} KB` : `${(bytes / 1048576).toFixed(1)} MB`;
    const label = asset => asset.name.replace(/\.[^.]+$/, '').replace(/_v\d+$/, '').replaceAll('_', ' ');
    const node = (tag, className, text) => { const element = document.createElement(tag); if (className) element.className = className; if (text !== undefined) element.textContent = text; return element; };
    function toast(message) { $('toast').textContent = message; $('toast').hidden = false; clearTimeout(toastTimer); toastTimer = setTimeout(() => { $('toast').hidden = true; }, 2600); }
    function persist() { try { localStorage.setItem(storageKey, JSON.stringify([...selected])); } catch { toast('Đã chọn trong phiên này; trình duyệt không cho lưu lâu dài.'); } }
    function toggle(asset) { selected.has(asset.path) ? selected.delete(asset.path) : selected.add(asset.path); persist(); render(); if (current) $('detailSelect').textContent = selected.has(current.path) ? 'Bỏ chọn asset' : 'Chọn asset'; }
    function matching() {
        const query = normalize($('search').value.trim());
        return assets.filter(asset => (!group || asset.group === group) && ($('mode').value !== 'images' || asset.image) && ($('mode').value !== 'selected' || selected.has(asset.path)) && ($('includeMeta').checked || !asset.path.endsWith('.meta')) && (!query || normalize(`${asset.path} ${asset.source} ${groups[asset.group]} ${asset.sprites.map(sprite => sprite.name).join(' ')}`).includes(query))).sort((a, b) => $('sort').value === 'size' ? b.bytes - a.bytes : $('sort').value === 'name' ? a.name.localeCompare(b.name) : a.path.localeCompare(b.path));
    }
    function render() {
        const mode = $('mode').value;
        $('metaControl').hidden = mode === 'images';
        $('selectionTools').hidden = mode !== 'selected';
        $('selectedCount').textContent = selected.size;
        $('groupTitle').textContent = mode === 'selected' ? 'Danh sách đã chọn' : groups[group] || 'Tất cả bộ sưu tập';
        const visible = matching();
        $('resultCount').textContent = `${visible.length} ${mode === 'images' ? 'ảnh' : 'file'}`;
        $('empty').hidden = visible.length > 0;
        $('grid').replaceChildren();
        const fragment = document.createDocumentFragment();
        for (const asset of visible) {
            const card = node('article', 'card' + (selected.has(asset.path) ? ' selected' : ''));
            const checkLabel = node('label', 'select-check');
            const check = document.createElement('input'); check.type = 'checkbox'; check.checked = selected.has(asset.path); check.setAttribute('aria-label', `Chọn ${asset.name}`); check.addEventListener('change', () => toggle(asset)); checkLabel.append(check);
            const preview = node('button', 'visual'); preview.setAttribute('aria-label', `Xem ${asset.name}`); preview.addEventListener('click', () => openDetail(asset));
            if (asset.image) { const image = document.createElement('img'); image.src = url(asset.path); image.alt = label(asset); image.loading = 'lazy'; image.decoding = 'async'; image.addEventListener('error', () => { image.hidden = true; preview.append(node('span', '', 'Không tải được ảnh')); }); preview.append(image); }
            else preview.append(node('span', 'file-icon', asset.extension.slice(1).toUpperCase() || 'FILE'));
            if (asset.sprites.length) preview.append(node('span', 'badge', `${asset.sprites.length} SPRITES`));
            const info = node('div', 'card-info'); info.append(node('h3', '', label(asset)));
            const meta = node('p'); meta.append(node('span', '', asset.width ? `${asset.width} × ${asset.height}` : asset.extension.slice(1).toUpperCase()), node('span', '', size(asset.bytes))); info.append(meta);
            card.append(checkLabel, preview, info); fragment.append(card);
        }
        $('grid').append(fragment);
        for (const button of $('categories').children) { const active = button.dataset.group === group; button.classList.toggle('active', active); button.setAttribute('aria-pressed', String(active)); }
    }
    function stopAnimation() { clearInterval(timer); timer = null; $('animate').textContent = 'Phát animation'; }
    function drawSprite(index) {
        if (!current || !current.image) return;
        const image = $('detailImage'), canvas = $('spriteCanvas');
        if (index < 0) { image.hidden = false; canvas.hidden = true; return; }
        const sprite = current.sprites[index];
        if (!sprite || !image.complete || !image.naturalWidth) return;
        canvas.width = sprite.width; canvas.height = sprite.height;
        canvas.getContext('2d').drawImage(image, sprite.x, image.naturalHeight - sprite.y - sprite.height, sprite.width, sprite.height, 0, 0, sprite.width, sprite.height);
        image.hidden = true; canvas.hidden = false;
    }
    function openDetail(asset) {
        stopAnimation(); current = asset;
        $('detailGroup').textContent = groups[asset.group]; $('detailTitle').textContent = asset.name;
        $('detailStats').textContent = `${asset.width ? `${asset.width} × ${asset.height} px · ` : ''}${size(asset.bytes)}${asset.ppu ? ` · ${asset.ppu} PPU` : ''}${asset.sprites.length ? ` · ${asset.sprites.length} sprites` : ''}`;
        $('archivePath').value = 'RawAssets/' + asset.path; $('sourcePath').value = asset.source;
        $('detailSelect').textContent = selected.has(asset.path) ? 'Bỏ chọn asset' : 'Chọn asset';
        $('detailImage').hidden = !asset.image; $('spriteCanvas').hidden = true;
        if (asset.image) { $('detailImage').src = url(asset.path); $('detailImage').alt = label(asset); } else $('detailImage').removeAttribute('src');
        $('spriteControl').hidden = !asset.sprites.length;
        $('spriteSelect').replaceChildren(new Option('Toàn bộ ảnh / sheet', '-1'), ...asset.sprites.map((sprite, index) => new Option(sprite.name, String(index))));
        const animated = asset.sprites.length > 1 && /_(flight|idle|cast)_v\d+/i.test(asset.name);
        $('animate').hidden = !animated; $('animationNote').hidden = !animated;
        $('openOriginal').href = url(asset.path); $('download').href = url(asset.path); $('download').download = asset.name;
        $('detail').showModal();
    }
    for (const [key, text] of [['', 'Tất cả'], ...Object.entries(groups)]) {
        const button = node('button'); button.dataset.group = key;
        button.append(node('span', '', text), node('span', '', assets.filter(asset => asset.image && (!key || asset.group === key)).length || '—'));
        button.addEventListener('click', () => { group = key; if (key === '09_Effects' || key === '10_Unity' || key === '13_Documentation') $('mode').value = 'all'; else if ($('mode').value === 'all') $('mode').value = 'images'; render(); }); $('categories').append(button);
    }
    $('totalImages').textContent = assets.filter(asset => asset.image).length;
    for (const id of ['mode', 'sort', 'includeMeta']) $(id).addEventListener('change', render);
    $('search').addEventListener('input', render);
    $('selectionView').addEventListener('click', () => { group = ''; $('search').value = ''; $('mode').value = 'selected'; render(); });
    $('background').addEventListener('change', () => { document.body.classList.remove('bg-dark', 'bg-light'); if ($('background').value !== 'checker') document.body.classList.add('bg-' + $('background').value); });
    $('resetFilters').addEventListener('click', () => { group = ''; $('search').value = ''; $('mode').value = 'images'; render(); });
    $('clearSelection').addEventListener('click', () => { selected.clear(); persist(); render(); });
    $('detailSelect').addEventListener('click', () => toggle(current));
    $('closeDetail').addEventListener('click', () => $('detail').close());
    $('detail').addEventListener('close', stopAnimation);
    $('detail').addEventListener('click', event => { if (event.target === $('detail')) { const rect = $('detail').getBoundingClientRect(); if (event.clientX < rect.left || event.clientX > rect.right || event.clientY < rect.top || event.clientY > rect.bottom) $('detail').close(); } });
    $('spriteSelect').addEventListener('change', () => { stopAnimation(); drawSprite(Number($('spriteSelect').value)); });
    $('detailImage').addEventListener('load', () => drawSprite(Number($('spriteSelect').value)));
    $('animate').addEventListener('click', () => {
        if (timer) { stopAnimation(); return; }
        let index = 0; $('animate').textContent = 'Dừng animation';
        const frame = () => { if (!current) return; $('spriteSelect').value = String(index); drawSprite(index); index = (index + 1) % current.sprites.length; if (/_cast_/i.test(current.name) && index === 0) stopAnimation(); };
        timer = setInterval(frame, /bullet_/.test(current.name) ? 1000 / 12 : /_cast_/.test(current.name) ? 100 : 1000 / 6); frame();
    });
    $('copyPath').addEventListener('click', async () => {
        try { if (!navigator.clipboard?.writeText) throw new Error('Clipboard unavailable'); await navigator.clipboard.writeText('RawAssets/' + current.path); toast('Đã sao chép đường dẫn.'); }
        catch { $('archivePath').focus(); $('archivePath').select(); toast('Đã chọn đường dẫn. Nhấn Ctrl+C để sao chép.'); }
    });
    $('exportSelection').addEventListener('click', () => {
        const items = assets.filter(asset => selected.has(asset.path)).map(asset => ({ name: asset.name, archive: 'RawAssets/' + asset.path, source: asset.source, sha256: asset.sha256 }));
        const blobUrl = URL.createObjectURL(new Blob([JSON.stringify({ exportedAt: new Date().toISOString(), assets: items }, null, 2)], { type: 'application/json' }));
        const link = document.createElement('a'); link.href = blobUrl; link.download = 'houkimajo-selected-assets.json'; link.click(); setTimeout(() => URL.revokeObjectURL(blobUrl), 1000); toast(`Đã xuất ${items.length} asset.`);
    });
    render();
})();

import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const root = path.dirname(fileURLToPath(import.meta.url));
const lines = fs.readFileSync(path.join(root, 'manifest.csv'), 'utf8').replace(/^\uFEFF/, '').trim().split(/\r?\n/);
const entries = lines.slice(1).map(line => {
    const values = [...line.matchAll(/"((?:[^"]|"")*)"/g)].map(match => match[1].replaceAll('""', '"'));
    if (values.length !== 4) throw new Error(`Invalid manifest row: ${line}`);
    const [source, archive, bytes, sha256] = values;
    const file = path.join(root, archive);
    if (!fs.existsSync(file)) throw new Error(`Missing asset: ${archive}`);
    const extension = path.extname(archive).toLowerCase();
    const image = ['.png', '.jpg', '.jpeg', '.webp', '.gif', '.bmp', '.svg'].includes(extension);
    const entry = { source, path: archive, bytes: Number(bytes), sha256, extension, image, group: archive.split('/')[0], name: path.basename(archive), sprites: [] };
    if (extension === '.png') {
        const header = fs.readFileSync(file).subarray(0, 24);
        entry.width = header.readUInt32BE(16);
        entry.height = header.readUInt32BE(20);
    }
    if (image && fs.existsSync(file + '.meta')) {
        const meta = fs.readFileSync(file + '.meta', 'utf8');
        entry.ppu = Number(meta.match(/spritePixelsToUnits: ([\d.]+)/)?.[1]) || null;
        const spriteSheet = meta.split('  spriteSheet:')[1] || '';
        for (const match of spriteSheet.matchAll(/\n\s+name: ([^\r\n]+)\r?\n\s+rect:\r?\n\s+serializedVersion: \d+\r?\n\s+x: ([\d.]+)\r?\n\s+y: ([\d.]+)\r?\n\s+width: ([\d.]+)\r?\n\s+height: ([\d.]+)/g)) {
            entry.sprites.push({ name: match[1].trim(), x: +match[2], y: +match[3], width: +match[4], height: +match[5] });
        }
    }
    return entry;
});
fs.writeFileSync(path.join(root, 'catalog-data.js'), 'window.ASSET_CATALOG = ' + JSON.stringify(entries).replaceAll('<', '\\u003c') + ';\n');
console.log(`Catalog: ${entries.length} files, ${entries.filter(entry => entry.image).length} images, ${entries.reduce((sum, entry) => sum + entry.sprites.length, 0)} sprite regions.`);

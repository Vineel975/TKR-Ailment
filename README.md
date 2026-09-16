const urls = performance.getEntriesByType('resource')
  .filter(r => r.name.includes('/_next/static/chunks/') && r.name.endsWith('.js'))
  .map(r => r.name);
for (const u of urls) {
  const t = await fetch(u).then(r => r.text());
  const m = t.match(/CLAIMAI_DIRECT_SAVE[^,;}]{0,40}/);
  if (m) console.log(u.split('/').pop(), '->', m[0]);
}

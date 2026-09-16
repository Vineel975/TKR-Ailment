const urls = performance.getEntriesByType('resource')
  .filter(r => r.name.includes('/_next/static/chunks/') && r.name.endsWith('.js'))
  .map(r => r.name);

const hits = await Promise.all(urls.map(u =>
  fetch(u).then(r => r.text()).then(t => ({
    u: u.split('/').pop(),
    directSave: t.includes('direct (bundle SP)'),
    entry: t.includes('handleSaveEntry'),
  }))
));
console.table(hits.filter(h => h.directSave || h.entry));

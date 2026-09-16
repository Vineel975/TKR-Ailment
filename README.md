const u = performance.getEntriesByType('resource')
  .map(r => r.name).find(n => n.includes('/_next/static/chunks/') && n.endsWith('.js'));

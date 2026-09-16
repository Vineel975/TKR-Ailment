const u = performance.getEntriesByType('resource')
  .map(r => r.name).find(n => n.includes('f70063540735db1a.js'));
const t = await fetch(u).then(r => r.text());
const i = t.indexOf('CLAIMAI_DIRECT_SAVE');
console.log(JSON.stringify(t.slice(i - 120, i + 120)));

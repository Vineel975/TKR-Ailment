await fetch('/api/claim/save', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: '{}'
}).then(r => r.json())

await fetch('/api/claim/save', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({
    claimId: "99999999999", slNo: 1, userId: 1,
    context: { roleId: 1, regionId: 1, createdUserRegionId: 1 }
  })
}).then(r => r.json())

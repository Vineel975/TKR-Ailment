# Spectra `Index.cshtml` — allow TKR through the AI Summary gate

File: `Views/MedicalScrutiny/Index.cshtml` (inline `<script>`), around line 7392, inside
`_doInitAiSummary`.

## Why
The classifier now returns `tkr`, but this client-side gate only lets `cataract`/`maternity`
through, so TKR shows: *"AI Summary is currently available only for Cataract and Maternity
claims. This claim (tkr) is not supported yet."* Add `tkr` to the allowed set.

This is the ONLY change needed in the cshtml — verified:
- `IsClaimAISummaryAllowed` (server) gates on ClaimTypeID/RequestTypeID, **not** disease — no change.
- The procedure-matching block (~line 9697) reads rules from `window._claimAI_rules[claimType]`,
  which is populated from `GET /api/rules` — that endpoint now includes `tkr` (registry:
  cataract, maternity, tkr, other), so TKR procedure rules flow automatically.
- Accommodation (`_claimAI_needsDayCare`, ~line 9550) is read only as a truthy check, so TKR
  (never set → falsy) already behaves like inpatient/maternity (no Day-care default). Correct as-is.

## Edit (line 7392)

### FIND
```javascript
            if (_claimType !== 'cataract' && _claimType !== 'maternity') {
                showAiError('AI Summary is currently available only for Cataract and Maternity claims. This claim (' + _claimType + ') is not supported yet.');
                return;
            }
```

### REPLACE WITH
```javascript
            if (_claimType !== 'cataract' && _claimType !== 'maternity' && _claimType !== 'tkr') {
                showAiError('AI Summary is currently available only for Cataract, Maternity and TKR claims. This claim (' + _claimType + ') is not supported yet.');
                return;
            }
```

## Deploy
This is inline script in the Razor view (not an external `.js`), so no `?v=` cache-buster is
needed — just deploy the updated `Index.cshtml` and recycle the app pool. Hard-refresh the
browser (Ctrl+F5) once to drop the old page from cache.

## Note (outage-only, optional)
The hardcoded procedure-rules fallback (~line 9756) defaults to cataract rules for any
non-maternity type if `GET /api/rules` is unreachable. With ClaimAI up, TKR uses the fetched
registry rules, so this only matters during a ClaimAI outage. Leave as-is unless you want a
hardcoded TKR fallback too.
